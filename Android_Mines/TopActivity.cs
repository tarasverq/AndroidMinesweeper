using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using Newtonsoft.Json;

namespace Android_Mines
{
    /// <summary>
    /// Activity отвечающая за топ игроков
    /// </summary>
    [Activity(Label = "Топ-10 игроков", Icon = "@drawable/icon")]
    public class TopActivity : Activity
    {
        private ISharedPreferences prefs;
        private List<KeyValuePair<string, int>> topDict;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Top);
            PrintTop();
        }

        /// <summary>
        /// Метод выгружает json, и десереализует его в словарь topDict
        /// </summary>
        private void GetTop()
        {
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            string json = prefs.GetString("Top-10", GetString(Resource.String.DefautTop));
            topDict = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(json);
        }

        /// <summary>
        /// Получает топ, и выводит его в виде таблицы в текстовое поле
        /// </summary>
        protected void PrintTop()
        {
            GetTop();
            //Сторонний класс для отрисовки текстовых таблиц
            ConsoleTable table = new ConsoleTable("#", "Имя", "Очки");
            int i = 1;
            //Сортируем, возможно появился новый лидер!
            foreach (KeyValuePair<string, int> current in topDict.OrderByDescending(x => x.Value).Take(10))
            {
                table.AddRow(i++, current.Key, current.Value);
            }

            EditText topText = FindViewById<EditText>(Resource.Id.TopText);
            topText.Text = table.ToStringAlternative();
        }

        /// <summary>
        /// Добавляет результаты текущего игрока в топ
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="points">Очки</param>
        protected void AddToTop(string name, int points)
        {
            topDict.Add(new KeyValuePair<string, int>(name, points));
            //Можно сразу сохранить
            SaveTop();
        }

        /// <summary>
        /// Сериализовывает словарь с топом и сохраняет в хранилище
        /// </summary>
        protected void SaveTop()
        {
            string json = JsonConvert.SerializeObject(topDict.OrderByDescending(x => x.Value).Take(10).ToList(), Formatting.Indented);
            prefs.Edit().PutString("Top-10", json).Apply();
        }



    }
}