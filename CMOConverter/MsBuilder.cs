using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace CMOConverter
{
    public class MsBuilder
    {
        public TextWriter Logger;

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
            // 結果フォルダ作成
            //
            var destDir = DateTime.Now.ToString("yyyyMMddHHmmss");
            //Utils.CreateResultDirectory(destDir);
            Directory.CreateDirectory(destDir);

            //var cursorLeft = 0;
            //var cursorTop = 0;

            var startBuildSignal = new ManualResetEventSlim();
            var cancelSource = new CancellationTokenSource();
            var cancelToken = cancelSource.Token;

            //
            // 処理中をコンソールに表示するためのタスク
            //   ビルドが開始されたタイミングでこのタスクも出力を初める
            //
            var showProcessingMarkTask = Task.Run(
                async () =>
                {
                    startBuildSignal.Wait(cancelToken);

                    do
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        //Utils.SetCursorPosition(cursorLeft, cursorTop);
                        //Utils.Write("★★処理中★★");
                        Logger.WriteLine("★★処理中★★");

                        await Task.Delay(TimeSpan.FromSeconds(1), cancelToken);
                        if (cancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        //Utils.SetCursorPosition(cursorLeft, cursorTop);
                        //Utils.Write("　　　　　　　");
                        Logger.WriteLine(".");

                        await Task.Delay(TimeSpan.FromSeconds(1), cancelToken);
                    }
                    while (true);
                },
                cancelToken);

            //
            // ビルドを実行するタスク
            //
            var buildTask = Task.Run(
                () =>
                {
                    //
                    // ビルド時の構成をマップで指定
                    //
                    var prop = new Dictionary<string, string>();

                    prop.Add("Configuration", "Debug" /*Utils.MsBuildConfiguration*/);
                    prop.Add("Platform", "Win32" /*Utils.MsBuildPlatform*/);

                    //
                    // ビルドリクエストを構築
                    //
                    var targets = /*Utils.MsBuildTargets*/ "_MeshContentTask".Split(new[] { "," }, StringSplitOptions.None);
                    var request = new BuildRequestData("../../../MakeCMO/MakeCMO.vcxproj" /*Utils.SlnFilePath*/, prop, null, targets, null);
                    //var req = new BuildRequestData(new ProjectInstance(), );

                    //
                    // ビルドパラメータを構築
                    //   パラメータ構築時にログファイル設定が行える
                    //
                    var projCollection = new ProjectCollection();
                    var parameter = new BuildParameters(projCollection);
                    parameter.Loggers = new List<ILogger> { new FileLogger { /*Parameters = Utils.MsBuildLogFileName*/ } };

                    //Utils.Write("ビルド開始....");
                    Logger.WriteLine("ビルド開始....");

                    //Interlocked.Exchange(ref cursorLeft, Utils.CursorLeft);
                    //Interlocked.Exchange(ref cursorTop, Utils.CursorTop);

                    startBuildSignal.Set();

                    //
                    // 最後にビルド実行を行ってくれるManagerオブジェクトを取得し、ビルド実行
                    //
                    var manager = BuildManager.DefaultBuildManager;
                    var result = manager.Build(parameter, request);

                    //
                    // 結果はOverallResultプロパティで判定できる
                    //
                    if (result.OverallResult == BuildResultCode.Failure)
                    {
                        // Utils.WriteLine(string.Empty);
                        // Utils.WriteLine("ビルド失敗");
                        Logger.WriteLine("ビルド失敗");

                        if (result.Exception != null)
                        {
                            //Utils.WriteLine(result.Exception.ToString());
                            Logger.WriteLine(result.Exception.ToString());
                            throw result.Exception;
                        }

                        throw new Exception("ビルドに失敗しました。");
                    }

                    //Utils.WriteLine(string.Empty);
                    //Utils.WriteLine("ビルド終了....");
                    Logger.WriteLine("ビルド終了....");
                });

            try
            {
                await buildTask;

                try
                {
                    cancelSource.Cancel();
                    await showProcessingMarkTask;
                }
                catch (TaskCanceledException)
                {
                    // noop
                }
            }
            catch (Exception ex)
            {
                //Utils.WriteLine("★★★★★ 処理中にエラーが発生しました ★★★★★");
                //Utils.WriteLine(ex.ToString());
                Logger.WriteLine("★★★★★ 処理中にエラーが発生しました ★★★★★");
                Logger.WriteLine(ex);

                cancelSource.Cancel();

                return;
            }

            // 出力先をオープン
            Process.Start(destDir);
        }
    }
}