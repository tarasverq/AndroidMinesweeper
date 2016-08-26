using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Text;

namespace Android_Mines
{
    [Activity(Label = "Сапёр", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public const string Prefix = "ru.tarasverq.minesweeper.";
        private ISharedPreferences prefs;
        private FieldAdapter fieldAdapter;
        private Random rand;
        private GridView field;
        private string name;
        private TextView statistics;
        private string statisticsFormat;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            name = "Аноним";
            statisticsFormat = "";
            rand = new Random((int) DateTime.Now.Ticks);
            prefs = GetPreferences(FileCreationMode.Private);
            SetContentView(Resource.Layout.Main);
            
            statistics = FindViewById<TextView>(Resource.Id.StatisticsLabel);
            field = FindViewById<GridView>(Resource.Id.Field);
            field.ItemClick += ItemClick;

            ShowNameAlert();
            ShowField();
        }

      

        /// <summary>
        /// Обработка клика по ячейке.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            fieldAdapter.FieldModel.ClickOnCell(e.Position);
            //Отрисовать грид заново.
            field.InvalidateViews();
            statistics.Text = string.Format(statisticsFormat, fieldAdapter.FieldModel.OpenedCells);
            switch (fieldAdapter.FieldModel.State)
            {
                case GameState.InGame:
                    break;
                case GameState.Fail:
                    RunEndGame(false);
                    //ShowAlert(false, fieldAdapter.FieldModel.Points);
                    break;
                case GameState.Won:
                    RunEndGame(true);
                    //ShowAlert(true, fieldAdapter.FieldModel.Points);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

      

        /// <summary>
        /// Создает новое игровое поле, адаптер, центрует GridView под текущие параметры.
        /// </summary>
        public void ShowField()
        {
            int fieldResolutionX = rand.Next(10, 20);
            int fieldResolutionY = rand.Next(10, 20);
            Console.WriteLine("Resolution: {0}:{1}", fieldResolutionX, fieldResolutionY);
            int rnd = rand.Next(12, 20); //Не трогать, иначе minesCount получается 0. Почему?
            int minesCount = (int) Math.Round(fieldResolutionX * fieldResolutionY / 100.0 * rnd);
            Console.WriteLine("Mines: {0}", minesCount);

            //Магия чтобы отцентровать GridView + подсчет оптимального размера ячейки.
            // http://stackoverflow.com/questions/6141910/android-center-gridview-horizontally
            var metrics = Resources.DisplayMetrics;
            int width_ = metrics.WidthPixels;
            int mSizePx = (int) (width_ / (fieldResolutionX * 1.05));
            mSizePx = Math.Min(mSizePx, (int) (metrics.HeightPixels * 0.75 / (fieldResolutionY * 1.05)));
            int contentWidth = fieldResolutionX * mSizePx; // Width of items
            int slack = (width_ - contentWidth) / 2;
            int vertPadding = (int) (10 * Resources.DisplayMetrics.Density);
            field.SetPadding(slack, vertPadding, slack, 0);

            field.SetNumColumns(fieldResolutionX);
            field.NumColumns = fieldResolutionX;
            fieldAdapter = new FieldAdapter(this, fieldResolutionX, fieldResolutionY, minesCount, mSizePx);
            field.Adapter = fieldAdapter;
            statisticsFormat = string.Format("Поле: {0}x{1}, мин: {2}.\r\nВсего клеток: {3}, открыто клеток:{{0}}.",
                fieldResolutionX, fieldResolutionY, minesCount, fieldResolutionX * fieldResolutionY);
            statistics.Text = string.Format(statisticsFormat, 0);
        }

        /// <summary>
        /// Окошко, запрашивающее имя
        /// </summary>
        private void ShowNameAlert()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle("Имя игрока");
            alert.SetMessage("Введите ваше имя");
            alert.SetCancelable(false);
            EditText input = new EditText(this)
            {
                Text = prefs.GetString("Name", ""),
                InputType = InputTypes.TextFlagAutoCorrect | InputTypes.TextFlagAutoComplete | InputTypes.TextFlagCapSentences
            };
            alert.SetView(input);

            alert.SetPositiveButton("Ok", delegate
            {
                name = input.Text;
                if (string.IsNullOrEmpty(name))
                {
                    Toast.MakeText(BaseContext, "Имя не может быть пустым.", ToastLength.Short).Show();
                    ShowNameAlert();
                    return;
                }
                prefs.Edit().PutString("Name", name).Apply();
            });


            alert.Show();
        }


        #region EndGame

        /// <summary>
        /// Запускает Activity EndGameActivity
        /// </summary>
        /// <param name="won">Выиграл?</param>
        private void RunEndGame(bool won)
        {
            Intent activity = new Intent(this, typeof(EndGameActivity)).SetFlags(ActivityFlags.ReorderToFront);
            activity.PutExtra(Prefix + "Won", won);
            activity.PutExtra(Prefix + "Points", fieldAdapter.FieldModel.Points);
            activity.PutExtra(Prefix + "Name", name);
            StartActivityForResult(activity, 0);
        }

        /// <summary>
        /// Обработка возвращения из EndGame активити.
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != 0 || resultCode != Result.Ok) return;

            //Результат: -1 - выход, 0 - новая игра, 1 - повтор
            int result = data.GetIntExtra(Prefix + "EndGameResult", -2);
            switch (result)
            {
                case -1:
                    Finish();
                    //System.Environment.Exit(0);
                    break;
                case 0:
                    ShowField();
                    break;
                case 1:
                    fieldAdapter.FieldModel.CloseField();
                    field.InvalidateViews();
                    break;
            }
        }
        #endregion EndGame

        #region Menu

        /// <summary>
        /// Отрисовывем меню
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            menu.Clear();
            MenuInflater.Inflate(Resource.Layout.Menu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        /// <summary>
        /// Обработка клика по меню
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.MainExitButton:
                    Finish();
                    return true;
                case Resource.Id.MainNewGameButton:
                    ShowField();
                    return true;
                case Resource.Id.MainRestartButton:
                    fieldAdapter.FieldModel.CloseField();
                    field.InvalidateViews();
                    return true;
                case Resource.Id.TopButton:
                    Intent activity = new Intent(this, typeof(TopActivity)).SetFlags(ActivityFlags.ReorderToFront);
                    StartActivity(activity);
                    return true;

            }
            return base.OnOptionsItemSelected(item);
        }
        #endregion Menu
    }
}

