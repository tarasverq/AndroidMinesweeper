using Android.Content;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace Android_Mines
{
    /// <summary>
    /// Адаптер чтоб подцепить игровое поле к GridView
    /// </summary>
    internal class FieldAdapter : BaseAdapter
    {
        private readonly Context context;
        private readonly int cellWidth;
        public Logic FieldModel { get; }
        public override int Count => FieldModel.Count;

        /// <summary>
        /// Массив с картинками полей без бомб
        /// </summary>
        private static readonly int[] CellImages = 
        {
            Resource.Drawable.Cell0,
            Resource.Drawable.Cell1,
            Resource.Drawable.Cell2,
            Resource.Drawable.Cell3,
            Resource.Drawable.Cell4,
            Resource.Drawable.Cell5,
            Resource.Drawable.Cell6,
            Resource.Drawable.Cell7,
            Resource.Drawable.Cell8
        };

        public FieldAdapter(Context context, int cols, int rows,int mines, int cellWidth)
        {
            //Ранее подобранная ширина картинки
            this.cellWidth = cellWidth;
            this.context = context;
            FieldModel = new Logic(cols, rows,  mines);
        }

        /// <summary>
        /// Выбирает изображение для каждой конкретной ячейки
        /// </summary>
        /// <param name="position"></param>
        /// <param name="convertView"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            
            ImageView view;
            //Или новый ImageView
            if (convertView == null)
            {
                //Задаем атрибуты картинки, размеры картинки в пикселях, тип масштабирования, отступы
                view = new ImageView(context) {LayoutParameters = new ViewGroup.LayoutParams(cellWidth, cellWidth) };
                view.SetAdjustViewBounds(false);
                view.SetScaleType(ImageView.ScaleType.CenterCrop);
                view.SetPadding(0, 0, 0, 0);
            }
            //Или обновляем существующий
            else view = (ImageView)convertView; 

            //Получаем текущую ячейку из класса с логикой
            Cell currentCell = FieldModel.GetCell(position);
            switch (currentCell.CellState)
            {
                case CellState.Closed:
                    //Закрытое поле
                    view.SetImageResource(Resource.Drawable.Closed);
                    break;
                case CellState.Opened:
                    switch (currentCell.MineState)
                    {
                        case MineState.No:
                            //Дергаем картинку из массива картинок без бомб
                            view.SetImageResource(CellImages[currentCell.MinesAround]);
                            break;
                        case MineState.Yes:
                            //BOMB!!!!!111
                            view.SetImageResource(Resource.Drawable.Bomb);
                            break;
                    }
                    break;
            }
            return view;
        }

        /// <summary>
        /// Заглушка
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override Object GetItem(int position)
        {

            return null;
        }

        /// <summary>
        /// Заглушка
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override long GetItemId(int position)
        {
            return position;
        }

    }
}