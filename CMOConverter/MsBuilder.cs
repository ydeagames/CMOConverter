using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMOConverter.Properties;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace CMOConverter
{
    public class MsBuilder
    {
        public TextBoxLogger Logger;
        public string[] Inputs;

        public Action OnBuildStarted = delegate { };
        public Action OnBuildFailed = delegate { };
        public Action OnBuildSucceed = delegate { };

        /// <summary>
        /// 処理を実行します。
        /// </summary>
        /// <returns>
        /// <see cref="Task" />オブジェクト
        /// </returns>
        /// <exception cref="Exception">
        /// 処理中にエラーが発生した場合
        /// </exception>
        public async Task Execute()
        {
            //
            // MsBuildをコードから実行する場合
            // 予め以下のDLLを参照設定しておく必要がある。
            //   ・Microsoft.Build.dll
            //   ・Microsoft.Build.Engine.dll
            //   ・Microsoft.Build.Framework.dll
            //

            //
            // ビルドを実行するタスク
            //
            var buildTask = Task.Run(
                () =>
                {
                    //
                    // ビルド時の構成
                    //
                    const string projectDirName = "Temp";
                    const string projectFileName = "Temp/MakeCMO.vcxproj";
                    if (Directory.Exists(projectDirName))
                        Directory.Delete(projectDirName, true);
                    Directory.CreateDirectory(projectDirName);
                    File.WriteAllText(projectFileName, Resources.Proj);

                    var proj = new ProjectInstance(projectFileName);

                    foreach (var input in Inputs)
                    {
                        var inPath = Path.GetFullPath(input);
                        var dir = Path.GetDirectoryName(inPath);
                        var name = Path.GetFileNameWithoutExtension(inPath);
                        var outPath = Path.Combine(dir ?? "", name + ".cmo");
                        var item = proj.AddItem("MeshContentTask", inPath);
                        item.SetMetadata("ContentOutput", outPath);
                        Logger.WriteLine($"入力ファイル: {inPath}, 出力ファイル: {outPath}");
                    }

                    //
                    // ビルドリクエストを構築
                    //
                    var request = new BuildRequestData(proj, new string[] { "_MeshContentTask" });

                    //
                    // ビルドパラメータを構築
                    //   パラメータ構築時にログファイル設定が行える
                    //
                    var parameter = new BuildParameters
                    {
                        Loggers = new List<ILogger>
                        {
                            Logger
                        }
                    };

                    //
                    // 最後にビルド実行を行ってくれるManagerオブジェクトを取得し、ビルド実行
                    //
                    var manager = BuildManager.DefaultBuildManager;
                    return manager.Build(parameter, request);
                });

            try
            {
                Logger.WriteLine("ビルド開始....");
                OnBuildStarted();

                var result = await buildTask;

                //
                // 結果はOverallResultプロパティで判定できる
                //
                if (result.OverallResult == BuildResultCode.Failure)
                {
                    Logger.WriteLine("ビルド失敗");
                    OnBuildFailed();

                    if (result.Exception != null)
                    {
                        Logger.WriteLine(result.Exception.ToString());
                        throw result.Exception;
                    }

                    return;
                }

                Logger.WriteLine("ビルド終了....");
                OnBuildSucceed();
            }
            catch (Exception ex)
            {
                Logger.WriteLine("処理中にエラーが発生しました");
                Logger.WriteLine(ex.ToString());

                return;
            }
        }
    }
}