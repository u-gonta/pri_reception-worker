using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Accessories;
using ReceptionWorker;

namespace Output
{
    /// <summary>
    /// 設定クラス
    /// </summary>
    [DataContract]
    public class Setting : ICloneable
    {
        /// <summary>
        /// 保存先
        /// </summary>
        [DataMember(Name = "Directory", Order = 1)]
        public string Directory { get; set; }

        /// <summary>
        /// 保存日数
        /// </summary>
        [DataMember(Name = "RetentionDays", Order = 2)]
        public int RetentionDays { get; set; }

        /// <summary>
        /// バッファ数
        /// </summary>
        [DataMember(Name = "BufferSize", Order = 3)]
        public int BufferSize { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Setting()
        {
            Directory = "";
            RetentionDays = 14;
            BufferSize = 100;
        }

        /// <summary>
        /// ディープコピー
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Setting ret = (Setting)this.MemberwiseClone();

            return ret;
        }
    }

    /// <summary>
    /// データクラス
    /// </summary>
    public class Data : ICloneable
    {
        /// <summary>
        /// 時刻
        /// </summary>
        public DateTime Clock { get; set; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Data(string message)
        {
            Clock = new DateTime();
            Clock = DateTime.Now;
            Message = message;
        }

        /// <summary>
        /// コピー
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Data ret = (Data)this.MemberwiseClone();

            return ret;
        }
    }

    /// <summary>
    /// 出力クラス ※処理の基礎クラスから派生
    /// </summary>
    public class Basis : Process.Basis
    {
        /// <summary>
        /// 区切り文字
        /// </summary>
        public const string ConstSeparator = "\t";

        /// <summary>
        /// ファイル名のフォーマット
        /// </summary>
        private const string Format = "yyyyMMdd";

        /// <summary>
        /// 動作の種類
        /// </summary>
        private enum EnumOperation
        {
            Wait,       // 待機
            Remove,     // 削除
            Write       // 書き込み
        }

        /// <summary>
        /// 設定クラスをロックするためのインスタンス
        /// </summary>
        private readonly object _asyncSetting = new object();

        /// <summary>
        /// 設定クラス
        /// </summary>
        private Setting _setting = new Setting();
        public Setting GetSetting()
        {
            Setting ret = null;

            lock (_asyncSetting)
            {
                ret = (Setting)_setting.Clone();
            }

            return ret;
        }
        public void SetSetting(Setting value)
        {
            lock (_asyncSetting)
            {
                _setting = (Setting)value.Clone();
            }
        }

        /// <summary>
        /// バッファーをロックするためのインスタンス
        /// </summary>
        private readonly object _asyncBuffer = new object();

        /// <summary>
        /// データクラス
        /// </summary>
        private readonly Queue<Data> _buffers = new Queue<Data>();
        private bool IsBuffer()
        {
            bool ret = false;

            // 排他制御
            lock (_asyncBuffer)
            {
                // バッファ数を確認
                if (0 < _buffers.Count)
                {
                    ret = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// 動作の種類をロックするためのインスタンス
        /// </summary>
        private readonly object _asyncOperation = new object();

        /// <summary>
        /// 動作の種類
        /// </summary>
        private EnumOperation _operation;
        private EnumOperation GetOperation()
        {
            EnumOperation ret = EnumOperation.Wait;

            // 排他制御
            lock (_asyncOperation)
            {
                ret = _operation;
            }

            return ret;
        }
        private void SetOperation(EnumOperation value)
        {
            // 排他制御
            lock (_asyncOperation)
            {
                _operation = value;
            }
        }

        /// <summary>
        /// 削除をロックするためのインスタンス
        /// </summary>
        private readonly object _asyncRemove = new object();

        /// <summary>
        /// 削除の要求フラグ
        /// </summary>
        private bool _requestRemove;
        private bool GetRequestRemove()
        {
            bool ret = false;

            // 排他制御
            lock (_asyncRemove)
            {
                ret = _requestRemove;
                _requestRemove = false;
            }

            return ret;
        }
        private void SetRequestRemove(bool value)
        {
            // 排他制御
            lock (_asyncRemove)
            {
                _requestRemove = value;
            }
        }

        /// <summary>
        /// 最終の保存日時
        /// </summary>
        private DateTime _saveLast = DateTime.Now;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Basis() : base("Output::")
        {
            // 削除の要求フラグを設定
            SetRequestRemove(true);
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        public override void  Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// 行動
        /// </summary>
        /// <returns></returns>
        protected override int Action()
        {
            int ret = base.Action();

            string title = App.Name + "Output::Action()" + ConstSeparator;

            try
            {
                switch (GetOperation())
                {
                    case EnumOperation.Wait:
                        // 待機 ⇒ 削除の要求を確認
                        if (GetRequestRemove())
                        {
                            // メイン関数の動作中フラグを更新
                            SetRunning(true);

                            // 削除へ
                            SetOperation(EnumOperation.Remove);
                            ret = 0;
                            break;
                        }

                        // バッファを確認
                        if (IsBuffer())
                        {
                            // メイン関数の動作中フラグを更新
                            SetRunning(true);

                            // 書き込みへ
                            SetOperation(EnumOperation.Write);
                            ret = 0;
                            break;
                        }

                        // メイン関数の動作中フラグを解除
                        SetRunning(false);

                        // 待機時間 ※1分
                        ret = 60 * 1000;
                        break;

                    case EnumOperation.Remove:
                        // 削除
                        ActionRemove();

                        // 待機へ
                        SetOperation(EnumOperation.Wait);
                        ret = 0;
                        break;

                    case EnumOperation.Write:
                        // 書き込み
                        ActionWrite();

                        // 待機へ
                        SetOperation(EnumOperation.Wait);
                        ret = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                Enqueue(new Data(Name + title + ex.Message));

                switch (GetOperation())
                {
                    case EnumOperation.Wait:
                    case EnumOperation.Remove:
                    case EnumOperation.Write:
                        // 待機 or 削除 or 出力 ⇒ 待機へ
                        SetOperation(EnumOperation.Wait);
                        break;
                }

                ret = 0;
            }

            return ret;
        }

        /// <summary>
        /// 削除
        /// </summary>
        private void ActionRemove()
        {
            string title;
            string message;

            title = App.Name + "Output::ActionRemove()" + ConstSeparator;

            Setting setting = GetSetting();

            do
            {
                string path;

                // ディレクトリを補正
                message = title + "Tool::CorrectionDirectory():" + setting.Directory;
                path = Tool.CorrectionDirectory(setting.Directory);

                // ディレクトリを確認
                if (Directory.Exists(path) == false)
                {
                    // ディレクトリなし ⇒ 削除を中断
                    break;
                }

                // 今日の日付を取得
                DateTime today = DateTime.Now;

                // 保存先をサーチ
                foreach (string file in Directory.EnumerateFiles(path, "*.log", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        string name;

                        message = title + "Path::GetFileNameWithoutExtension():" + file;
                        name = Path.GetFileNameWithoutExtension(file);

                        // ファイル名から日数を取得
                        message = title + "DateTime.ParseExact():" + name;
                        DateTime target = DateTime.ParseExact(name, Format, CultureInfo.InvariantCulture);
                        double duration = (today - target).TotalDays - 1;

                        // 経過日数を確認
                        if (setting.RetentionDays < duration)
                        {
                            // ファイル削除
                            message = title + "File.Delete():" + file;
                            File.Delete(file);

                            Enqueue(new Data(Name + title + name));
                        }
                    }
                    catch (Exception ex)
                    {
                        // 例外の処理 ⇒ 出力に追加
                        Enqueue(new Data(Name + message + ConstSeparator + ex.Message));
                    }
                }
            } while (false);
        }

        /// <summary>
        /// 書き込み
        /// </summary>
        private void ActionWrite()
        {
            string title;
            string message;

            title = App.Name + "ActionWrite()" + ConstSeparator;
            message = title;

            Setting setting = GetSetting();
            bool valid;
            string pathLast;
            StreamWriter writer = null;
            string path;

            valid = false;
            pathLast = "";

            // ディレクトリを確認
            if (string.IsNullOrEmpty(setting.Directory) == false)
            {
                try
                {
                    // パスを補正
                    message = title + "Tool::CorrectionDirectory():" + setting.Directory;
                    path = Tool.CorrectionDirectory(setting.Directory);

                    // ディレクトリ作成
                    message = title + "Directory::CreateDirectory():" + path;
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    valid = true;
                }
                catch (Exception ex)
                {
                    // 例外の処理 ⇒ 出力に追加
                    Enqueue(new Data(Name + message + ConstSeparator + ex.Message));
                }
            }

            try
            {
                while (true)
                {
                    Data buffer = null;

                    // 排他制御
                    message = title + "バッファを取得";
                    lock (_asyncBuffer)
                    {
                        // バッファ数を確認
                        if (_buffers.Count <= 0)
                        {
                            break;
                        }

                        // 先頭のデータを取得
                        buffer = (Data)_buffers.Dequeue().Clone();
                    }

                    // 有効フラグを確認
                    if (valid == false)
                    {
                        continue;
                    }

                    message = title + "Tool::CorrectionDirectory():" + setting.Directory;
                    path = Tool.CorrectionDirectory(setting.Directory);
                    path += buffer.Clock.ToString(Format) + ".log";

                    // 最後のファイルパスを比較
                    if (pathLast != path)
                    {
                        // オープンを確認
                        if (writer != null)
                        {
                            writer.Close();
                            writer.Dispose();
                            writer = null;
                        }

                        message = title + "StreamWriter():" + path;
                        writer = new StreamWriter(path, true, Encoding.UTF8);

                        // 最終のファイルパスを更新
                        pathLast = path;
                    }

                    // オープンを確認
                    if (writer == null)
                    {
                        // 例外を発砲
                        throw new Exception(path + ":writerが空");
                    }

                    // ファイルへ書き込み
                    message = title + "StreamWriter::WriteLine():" + buffer.Message;
                    writer.WriteLine(buffer.Message);
                    Trace.WriteLine(buffer.Message);
                    continue;
                }
            }
            catch (Exception ex)
            {
                // 例外の処理 ⇒ デバッグ出力
                Trace.WriteLine(Name + message + ConstSeparator + ex.Message);
            }
            finally
            {
                // 破棄
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
            }

            DateTime last = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            // 最終の保存日時からの経過時間
            TimeSpan span = last - _saveLast;

            // 経過日数を確認
            if (0 < span.TotalDays)
            {
                // 削除の要求フラグを設定
                SetRequestRemove(true);
                // 最終の保存日時を設定
                _saveLast = last;
            }
        }

        /// <summary>
        /// バッファデータを追加
        /// </summary>
        /// <param name="value">バッファクラス</param>
        public void Enqueue(Data value)
        {
            // 排他制御
            lock (_asyncBuffer)
            {
                // データを登録
                _buffers.Enqueue((Data)value.Clone());

                // バッファ数を確認
                if (GetSetting().BufferSize < _buffers.Count)
                {
                    // 要求
                    if (_request.Set() == false)
                    {
                        throw new Exception("要求失敗");
                    }
                }
            }
        }
    }
}
