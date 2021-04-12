using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ReceptionWorker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_STYLE = -16;
        const int WS_SYSMENU = 0x80000;

        /// <summary>
        /// ウィンドウの初期位置
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(handle, GWL_STYLE);
            style &= (~WS_SYSMENU);
            SetWindowLong(handle, GWL_STYLE, style);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string title = App.Name + "MainWindow::Window_Loaded()" + Output.Basis.ConstSeparator;
            string message = "";

            Output.Manager outputManager = Output.Manager.Instance;

            try
            {
                Setting.Manager settingManager = Setting.Manager.Instance;

                // 更新
                message = title + "Update()";
                Update(settingManager.Data);
                outputManager.Enqueue(new Output.Data(message));
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ エラーメッセージを付加
                message += Output.Basis.ConstSeparator + ex.Message;
                outputManager.Enqueue(new Output.Data(message));
            }
        }

        /// <summary>
        /// 更新ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            String title = App.Name + "MainWindow::ButtonOk_Click()" + Output.Basis.ConstSeparator;
            String message = "";

            Output.Manager outputManager = Output.Manager.Instance;

            try
            {
                Setting.Manager settingManager = Setting.Manager.Instance;

                // 取得
                message = title + "GetSetting()";
                settingManager.Data = GetSetting();

                // 保存
                message = title + "settingManager.FileWrite()";
                settingManager.FileWrite();
                outputManager.Enqueue(new Output.Data(message));

                // 更新
                message = title + "outputManager.Update()";
                outputManager.Update(settingManager.Data.GetOutput());

                UdpServer.Manager udpServerManager = UdpServer.Manager.Instance;

                // 開始
                message = title + "udpServerManager.Start()";
                udpServerManager.Start(settingManager.Data.GetServers());
                outputManager.Enqueue(new Output.Data(message));
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ エラーメッセージを付加
                message += Output.Basis.ConstSeparator + ex.Message;
                outputManager.Enqueue(new Output.Data(message));
            }
        }

        /// <summary>
        /// 閉じるボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            String title = App.Name + "MainWindow::ButtonClose_Click()" + Output.Basis.ConstSeparator;
            String message = "";

            Output.Manager outputManager = Output.Manager.Instance;

            message = title + "Close()";
            outputManager.Enqueue(new Output.Data(message));

            this.Close();
        }

        /// <summary>
        /// 通信設定コントロールを取得
        /// </summary>
        /// <param name="target">UDPサーバーの種類</param>
        /// <returns></returns>
        private SettingCommunication Communication(UdpServer.Manager.EnumKind target)
        {
            SettingCommunication ret = null;

            switch (target)
            {
                case UdpServer.Manager.EnumKind.First:
                    ret = SettingCommunication01;
                    break;

                case UdpServer.Manager.EnumKind.Second:
                    ret = SettingCommunication02;
                    break;

                case UdpServer.Manager.EnumKind.Third:
                    ret = SettingCommunication03;
                    break;

                case UdpServer.Manager.EnumKind.Fourth:
                    ret = SettingCommunication04;
                    break;

                case UdpServer.Manager.EnumKind.Fifth:
                    ret = SettingCommunication05;
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 設定クラスを取得
        /// </summary>
        private Setting.Basis GetSetting()
        {
            Setting.Basis ret = new Setting.Basis();

            Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting> udpServers = new Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting>();

            foreach (UdpServer.Manager.EnumKind target in Enum.GetValues(typeof(UdpServer.Manager.EnumKind)))
            {
                SettingCommunication control = Communication(target);

                if (control == null)
                {
                    continue;
                }

                udpServers[target] = (UdpServer.Setting)control.GetServer().Clone();
            }
            ret.SetServers(new Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting>(udpServers));

            Output.Setting output = new Output.Setting
            {
                Directory = textBoxOutputDirectory.Text,
                RetentionDays = int.Parse(textBoxOutputRetentionDays.Text),
                BufferSize = int.Parse(textBoxOutputBufferSize.Text)
            };
            ret.SetOutput((Output.Setting)output.Clone());

            return ret;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="parameter">設定クラス</param>
        private void Update(Setting.Basis setting)
        {
            Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting> udpServers = setting.GetServers();

            foreach (UdpServer.Manager.EnumKind target in Enum.GetValues(typeof(UdpServer.Manager.EnumKind)))
            {
                SettingCommunication control = Communication(target);

                if (control == null)
                {
                    continue;
                }
                control.SetServer(udpServers[target]);
            }

            Output.Setting output = setting.GetOutput();

            textBoxOutputDirectory.Text = output.Directory;
            textBoxOutputRetentionDays.Text = output.RetentionDays.ToString();
            textBoxOutputBufferSize.Text = output.BufferSize.ToString();
        }
    }
}
