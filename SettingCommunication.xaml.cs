using System.Windows.Controls;

namespace ReceptionWorker
{
    /// <summary>
    /// SettingCommunication.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingCommunication : UserControl
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingCommunication()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 取得
        /// </summary>
        public UdpServer.Setting GetServer()
        {
            UdpServer.Setting ret = new UdpServer.Setting();

            ret.Valid = checkBoxValid.IsChecked.Value;
            ret.Title = textBoxTitle.Text;
            ret.Address = textBoxAddress.Text;
            ret.Port = int.Parse(textBoxPort.Text);

            return ret;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="value">設定クラス</param>
        public void SetServer(UdpServer.Setting value)
        {
            checkBoxValid.IsChecked = value.Valid;
            textBoxTitle.Text = value.Title;
            textBoxAddress.Text = value.Address;
            textBoxPort.Text = value.Port.ToString();
        }
    }
}
