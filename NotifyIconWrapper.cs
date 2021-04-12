using System;
using System.ComponentModel;
using System.Windows;

namespace ReceptionWorker
{
    /// <summary>
    /// タスクトレイ通知アイコン
    /// </summary>
    public partial class NotifyIconWrapper : Component
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NotifyIconWrapper()
        {
            InitializeComponent();

            // 名称を更新
            this.toolStripMenuItemSetting.Text = Properties.Resources.Setting;
            this.toolStripMenuItemExit.Text = Properties.Resources.Exit;

            // コンテキストメニューのイベントを設定
            this.toolStripMenuItemSetting.Click += this.ToolStripMenuItemSetting_Click;
            this.toolStripMenuItemExit.Click += this.ToolStripMenuItemExit_Click;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="container"></param>
        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// コンテキストメニュー "設定" を選択したとき呼ばれます。
        /// </summary>
        /// <param name="sender">呼び出し元オブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void ToolStripMenuItemSetting_Click(object sender, EventArgs e)
        {
            MainWindow window;

            try
            {
                window = new MainWindow();

                // 表示
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// コンテキストメニュー "終了" を選択したとき呼ばれます。
        /// </summary>
        /// <param name="sender">呼び出し元オブジェクト</param>
        /// <param name="e">イベントデータ</param>
        private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            // 現在のアプリケーションを終了
            Application.Current.Shutdown();
        }
    }
}
