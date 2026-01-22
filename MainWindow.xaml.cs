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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Facienda2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Root _root; // 全データを格納するオブジェクト
        private const string JSON_FN = "facienda.json"; // データ保存用JSON
        private const string JSON_BK_FN = "facienda_bk.json"; // アプリ起動時に作成されるバックアップ
        private bool _isDragging = false; // ドラッグ中フラグ
        private Point _startPosition; // ドラッグ開始位置
        private Point _lastPosition; // ドラッグ中の位置
        private TaskItem _draggingTask; // ドラッグ中のタスク
        private BannerItem _draggingBanner; // ドラッグ中のバナー

        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists(JSON_FN))
            {
                File.Create(JSON_FN).Close(); // Jsonがなければ作成
            }
            File.Copy(JSON_FN, JSON_BK_FN, true); // Jsonのバックアップ
            LoadJson();
            DataContext = _root;
            Width = _root.Settings.Width; // ウィンドウ幅の設定
            Height = _root.Settings.Height; // ウィンドウ高さの設定
        }

        // Jsonからのデータ読み込み
        private void LoadJson()
        {
            var json = File.ReadAllText("facienda.json");
            _root = JsonConvert.DeserializeObject<Root>(json);

            // _rootがNullの場合は初期化しておく
            if (_root == null) { _root = new Root(); }
            if (_root.Tasks == null) { _root.Tasks = new ObservableCollection<TaskItem>(); }
            if (_root.Banners == null) { _root.Banners = new ObservableCollection<BannerItem>(); }
            if (_root.Settings == null) { _root.Settings = new Settings() { Width = 900, Height = 600 }; }
        }

        // Jsonへのデータ保存
        private void SaveJson()
        {
            var json = JsonConvert.SerializeObject(_root, Formatting.Indented);
            File.WriteAllText("facienda.json", json);
        }

        // タスクのリネーム
        public void RenameTask(TaskItem task, string name)
        {
            task.Name = name;
            SaveJson();
        }

        // バナーのリネーム
        public void RenameBanner(BannerItem banner, string name)
        {
            banner.Name = name;
            SaveJson();
        }

        // タスクの期日設定
        public void SetDueDate(TaskItem task, DateTime? date)
        {
            task.DueDate = date;
            SaveJson();
        }

        // 完了タスクのクリア
        private void ClearTasks()
        {
            var target = _root.Tasks.ToList();
            foreach (TaskItem task in target)
            {
                if (task.IsDone)
                {
                    DeleteTask(task);
                }
            }
        }

        // タスクの削除
        private void DeleteTask(TaskItem task)
        {
            _root.Tasks.Remove(task); // Rootのリストから削除
            SaveJson();
        }

        // バナーの削除
        private void DeleteBanner(BannerItem banner)
        {
            _root.Banners.Remove(banner); // Rootのリストから削除
            SaveJson();
        }

        // タスクの新規作成
        private void CreateTask(string name)
        {
            TaskItem newtask = new TaskItem();
            newtask.Name = name;
            newtask.Id = Guid.NewGuid().ToString();
            newtask.IsDone = false;
            newtask.PositionX = 100;
            newtask.PositionY = 100;
            _root.Tasks.Add(newtask);
            SaveJson();
            OpenTaskWindow(newtask); // タスク作成直後に命名をさせる
        }

        // バナーの新規作成
        private void CreateBanner(string name)
        {
            BannerItem newbanner = new BannerItem();
            newbanner.Name = name;
            newbanner.Id = Guid.NewGuid().ToString();
            newbanner.PositionX = 100;
            newbanner.PositionY = 100;
            _root.Banners.Add(newbanner);
            SaveJson();
            OpenBannerWindow(newbanner); // バナー作成直後に命名をさせる
        }

        // タスク編集ウィンドウの起動
        private void OpenTaskWindow(TaskItem task)
        {
            if (task == null) { return; } // タスクが指定されていない場合は処理を終了する
            var dlg = new TaskDetailWindow(task);
            dlg.owner = this;
            dlg.Owner = this; // メインウィンドウ中央に出すため
            dlg.ShowDialog();
        }

        // バナー編集ウィンドウの起動
        private void OpenBannerWindow(BannerItem banner)
        {
            if (banner == null) { return; } // バナーが指定されていない場合は処理を終了する
            var dlg = new BannerDetailWindow(banner);
            dlg.owner = this;
            dlg.Owner = this; // メインウィンドウ中央に出すため
            dlg.ShowDialog();
        }

        // 以下はイベントハンドラ

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateTask("New Task");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CreateBanner("New Banner");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ClearTasks();
        }

        // タスクカードドラッグのためのイベントハンドラたち
        private void TaskCard_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is TaskItem task)
            {
                _isDragging = false;
                _draggingTask = task;
                _startPosition = e.GetPosition(this);
                _lastPosition = e.GetPosition(this);
                checkBox.CaptureMouse();
                // e.Handled = true; // 完了ステータスのトグルを防ぐ
            }
        }
        private void TaskCard_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggingTask != null && sender is CheckBox checkBox)
            {
                Point currentPosition = e.GetPosition(this);
                double offsetX = currentPosition.X - _lastPosition.X;
                double offsetY = currentPosition.Y - _lastPosition.Y;
                _draggingTask.PositionX += (int)offsetX;
                _draggingTask.PositionY += (int)offsetY;
                _lastPosition = currentPosition;
                Vector fromStart = currentPosition - _startPosition;
                if (fromStart.Length > 3)
                {
                    _isDragging = true;
                }
            }
        }
        private void TaskCard_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                checkBox.ReleaseMouseCapture();
                if (!_isDragging)
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                }
                _isDragging = false;
                _draggingTask = null;
                SaveJson(); // ドラッグを終了したら位置をJsonに保存する
            }
        }

        // バナーラベルドラッグのためのイベントハンドラたち
        private void BannerLabel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is BannerItem banner)
            {
                _draggingBanner = banner;
                _startPosition = e.GetPosition(this);
                _lastPosition = e.GetPosition(this);
                border.CaptureMouse();
            }
        }
        private void BannerLabel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggingBanner != null && sender is Border border)
            {
                Point currentPosition = e.GetPosition(this);
                double offsetX = currentPosition.X - _lastPosition.X;
                double offsetY = currentPosition.Y - _lastPosition.Y;
                _draggingBanner.PositionX += (int)offsetX;
                _draggingBanner.PositionY += (int)offsetY;
                _lastPosition = currentPosition;
            }
        }
        private void BannerLabel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                border.ReleaseMouseCapture();
                _draggingBanner = null;
                SaveJson(); // ドラッグを終了したら位置をJsonに保存する
            }
        }

        // コンテキストメニューのハンドラ
        private void MenuItem_SetDueDate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.DataContext is TaskItem task)
            {
                // ダイアログで日付を選択させる実装
                // 例: DatePickerダイアログを表示
                var dialog = new Window
                {
                    Title = "Due Date",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var datePicker = new DatePicker
                {
                    SelectedDate = task.DueDate,
                    Margin = new Thickness(20)
                };

                var button = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(20, 0, 20, 20)
                };

                button.Click += (s, args) =>
                {
                    SetDueDate(task, datePicker.SelectedDate);
                    dialog.Close();
                };

                var panel = new StackPanel();
                panel.Children.Add(datePicker);
                panel.Children.Add(button);
                dialog.Content = panel;

                dialog.ShowDialog();
            }
        }
        private void MenuItem_DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.DataContext is TaskItem task)
            {
                DeleteTask(task);
            }
        }

        private void MenuItem_DeleteBanner_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.DataContext is BannerItem banner)
            {
                DeleteBanner(banner);
            }
        }

        // ウィンドウ終了時のイベントハンドラ
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _root.Settings.Width = Width;
            _root.Settings.Height = Height;
            SaveJson();
        }
    }

    // 日付から曜日を出すコンバータ
    public class DateToDayOfWeekConverter : IValueConverter
    {
        // Dateを曜日に変換
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                string dayOfWeek = date.ToString("ddd", new CultureInfo("en-US"));
                return "(" + dayOfWeek + ")";
            }
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
