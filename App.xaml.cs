using System;
using System.Diagnostics;
using System.Windows;

namespace ReceptionWorker
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// タスクトレイに表示するアイコン
        /// </summary>
        private NotifyIconWrapper notifyIcon;

        /// <summary>
        /// ソフト名 ※先頭の区切り付き
        /// </summary>
        public const string Name = "ReceptionWorker" + Output.Basis.ConstSeparator;

        /// <summary>
        /// System.Windows.Application.Startup イベント を発生させます。
        /// </summary>
        /// <param name="e">イベントデータ を格納している StartupEventArgs</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 言語切り替え
            //@@@ResourceService.Current.ChangeCulture("en-US");

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.notifyIcon = new NotifyIconWrapper();

            Setting.Manager settingManager = Setting.Manager.Instance;
            string title = Name + "OnStartup()" + Output.Basis.ConstSeparator;
            string message = "";

            try
            {
                // ファイル読み込み
                message = title + "settingManager.FileRead()";
                settingManager.FileRead();
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ エラーメッセージを付加
                message += Output.Basis.ConstSeparator + ex.Message;
            }
            Trace.WriteLine(message);

            Output.Manager outputManager = Output.Manager.Instance;

            try
            {
                // 出力を開始
                message = title + "outputManager.Start()";
                outputManager.Start(settingManager.Data.GetOutput());
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ エラーメッセージを付加
                message += Output.Basis.ConstSeparator + ex.Message;
            }
            Trace.WriteLine(message);

            UdpServer.Manager udpServerManager = UdpServer.Manager.Instance;

            try
            {
                // 通信を開始
                message = title + "updServerManager.Start()";
                udpServerManager.Start(settingManager.Data.GetServers());
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ エラーメッセージを付加
                message += Output.Basis.ConstSeparator + ex.Message;
            }
            Trace.WriteLine(message);
        }

        /// <summary>
        /// System.Windows.Application.Exit イベント を発生させます。
        /// </summary>
        /// <param name="e">イベントデータ を格納している ExitEventArgs</param>
        protected override void OnExit(ExitEventArgs e)
        {
            UdpServer.Manager udpServerManager = UdpServer.Manager.Instance;
            string title = Name + "OnExit()" + Output.Basis.ConstSeparator;
            string message = "";

            try
            {
                // 通信を停止
                message = title + "udpServerManager.Stop()";
                udpServerManager.Stop();
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ エラーメッセージを付加
                message += Output.Basis.ConstSeparator + ex.Message;
            }
            Trace.WriteLine(message);

            Output.Manager outputManager = Output.Manager.Instance;

            try
            {
                // 出力を停止
                message = title + "outputManager.Stop()";
                outputManager.Stop();
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ エラーメッセージを付加
                message += Output.Basis.ConstSeparator + ex.Message;
            }
            Trace.WriteLine(message);

            base.OnExit(e);

            this.notifyIcon.Dispose();
        }
    }
}
