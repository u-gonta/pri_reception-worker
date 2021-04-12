using ReceptionWorker.Properties;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ReceptionWorker
{
    /// <summary>
    /// 多言語化されたリソースと、言語の切り替え機能を提供します。
    /// </summary>
    public class ResourceService
    {
        #region singleton members

        private static readonly ResourceService _current = new ResourceService();
        public static ResourceService Current
        {
            get { return _current; }
        }

        #endregion

        private readonly Properties.Resources _resources = new Resources();

        /// <summary>
        /// 多言語化されたリソースを取得します。
        /// </summary>
        public Properties.Resources Resources
        {
            get { return this._resources; }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// 指定されたカルチャ名を使用して、リソースのカルチャを変更します。
        /// </summary>
        /// <param name="name">カルチャの名前。</param>
        public void ChangeCulture(string name)
        {
            Resources.Culture = CultureInfo.GetCultureInfo(name);
            this.RaisePropertyChanged("Resources");
        }
    }
}
