using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace Android_Mines
{
    /// <summary>
    /// Activity результатов игры, наследуется от TopActivity
    /// </summary>
    [Activity(Label = "@string/GameResults", Icon = "@drawable/Icon", ScreenOrientation = ScreenOrientation.Portrait)]
    class EndGameActivity : TopActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EndGame);
            TextView resultsLabel = FindViewById<TextView>(Resource.Id.ResultsLabel);
            Button exitBtn = FindViewById<Button>(Resource.Id.ExitButton);
            Button restartBtn = FindViewById<Button>(Resource.Id.RestartButton);
            Button newGameBtn = FindViewById<Button>(Resource.Id.StartNewGameButton);

            exitBtn.Click += OnExitClick;
            restartBtn.Click += OnRestartClick;
            newGameBtn.Click += OnNewGameClick;

            //Получаем значения переменых
            string name = Intent.GetStringExtra(MainActivity.Prefix + "Name") ?? GetString(Resource.String.Anonimous);
            name = string.IsNullOrWhiteSpace(name) ? GetString(Resource.String.Anonimous) : name;
            int points = Intent.GetIntExtra(MainActivity.Prefix + "Points", 0);
            bool won = Intent.GetBooleanExtra(MainActivity.Prefix + "Won", false);

            //Выводим результат, обновляем и сохраняем топ
            resultsLabel.Text = string.Format(GetString(Resource.String.ResultsFormat),
                won ? GetString(Resource.String.Winner) : GetString(Resource.String.Loser), points);
            AddToTop(name, points);
            PrintTop();
            SaveTop();
        }

        private void OnRestartClick(object sender, EventArgs e)
        {
          SendResult(1);
        }

        private void OnNewGameClick(object sender, EventArgs e)
        {
           SendResult(0);
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            SendResult(-1);
        }

        /// <summary>
        /// Передает управление назад в главный Activity вместе с выбранной кнопкой
        /// </summary>
        /// <param name="result">Результат: -1 - выход, 0 - новая игра, 1 - повтор</param>
        private void SendResult(int result)
        {
            Intent answerIntent = new Intent();
            answerIntent.PutExtra(MainActivity.Prefix + "EndGameResult", result);
            SetResult(Result.Ok, answerIntent);
            Finish();
        }


    }
}