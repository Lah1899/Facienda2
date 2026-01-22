using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Facienda2
{
    /// <summary>
    /// TaskDetailWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BannerDetailWindow : Window
    {
        private BannerItem _banner;
        public MainWindow owner;

        public BannerDetailWindow(BannerItem banner)
        {
            InitializeComponent();
            this._banner = banner;

            // ウィンドウ起動時にタスク名を入れておく
            InputBox.Text = _banner.Name;

            // バナー名を全選択した状態でフォーカス
            this.Loaded += (s, e) =>
            {
                InputBox.Focus();
                InputBox.SelectAll();
            };
        }

        // Enterが押されたらバナーのリネームを実行する
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var input = InputBox.Text;
                owner.RenameBanner(_banner, input);

                // モーダルを閉じたいならこれで戻る
                DialogResult = true;
                Close();
            }
        }
    }
}
