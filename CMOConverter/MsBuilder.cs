﻿using System;
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
        public TextBoxLogger Logger;
        public string[] Inputs;

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

                        Logger.WriteLine("変換開始");

                        await Task.Delay(TimeSpan.FromSeconds(1), cancelToken);
                        if (cancelToken.IsCancellationRequested)
                        {
                            break;
                        }

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
                    // ビルド時の構成
                    //
                    const string projectFileName = "../../../MakeCMO/MakeCMO.vcxproj";

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
                    var request = new BuildRequestData(proj, new string[] {"_MeshContentTask"});

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

                    Logger.WriteLine("ビルド開始....");

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
                        Logger.WriteLine("ビルド失敗");

                        if (result.Exception != null)
                        {
                            //Utils.WriteLine(result.Exception.ToString());
                            Logger.WriteLine(result.Exception.ToString());
                            throw result.Exception;
                        }

                        throw new Exception("ビルドに失敗しました。");
                    }

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
                Logger.WriteLine("処理中にエラーが発生しました");
                Logger.WriteLine(ex.ToString());

                cancelSource.Cancel();

                return;
            }
        }
    }
}