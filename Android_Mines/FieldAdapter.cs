using Android.Content;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace Android_Mines
{
    /// <summary>
    /// ������� ���� ��������� ������� ���� � GridView
    /// </summary>
    internal class FieldAdapter : BaseAdapter
    {
        private readonly Context context;
        private readonly int cellWidth;
        public Logic FieldModel { get; }
        public override int Count => FieldModel.Count;

        /// <summary>
        /// ������ � ���������� ����� ��� ����
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
            //����� ����������� ������ ��������
            this.cellWidth = cellWidth;
            this.context = context;
            FieldModel = new Logic(cols, rows,  mines);
        }

        /// <summary>
        /// �������� ����������� ��� ������ ���������� ������
        /// </summary>
        /// <param name="position"></param>
        /// <param name="convertView"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            
            ImageView view;
            //��� ����� ImageView
            if (convertView == null)
            {
                //������ �������� ��������, ������� �������� � ��������, ��� ���������������, �������
                view = new ImageView(context) {LayoutParameters = new ViewGroup.LayoutParams(cellWidth, cellWidth) };
                view.SetAdjustViewBounds(false);
                view.SetScaleType(ImageView.ScaleType.CenterCrop);
                view.SetPadding(0, 0, 0, 0);
            }
            //��� ��������� ������������
            else view = (ImageView)convertView; 

            //�������� ������� ������ �� ������ � �������
            Cell currentCell = FieldModel.GetCell(position);
            switch (currentCell.CellState)
            {
                case CellState.Closed:
                    //�������� ����
                    view.SetImageResource(Resource.Drawable.Closed);
                    break;
                case CellState.Opened:
                    switch (currentCell.MineState)
                    {
                        case MineState.No:
                            //������� �������� �� ������� �������� ��� ����
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
        /// ��������
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override Object GetItem(int position)
        {

            return null;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override long GetItemId(int position)
        {
            return position;
        }

    }
}