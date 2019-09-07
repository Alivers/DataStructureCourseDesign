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
using System.Windows.Threading;
using System.Reflection;

namespace SortVisualization
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private SortingView myView;
        public DispatcherTimer WholeTimer = new DispatcherTimer();
        public DispatcherTimer SwapTimer = new DispatcherTimer();
        public DispatcherTimer MoveTimer = new DispatcherTimer();

        private const string InputPrompt = "请输入1-50之间的数(15-20)个(用空格分隔), 无输入则随机生成";
        public MainWindow()
        {
            InitializeComponent();
            SetDefaultText();
            this.myView = new SortingView(this.DemoArea.Width, this.DemoArea.Height, this.WaitArea.Width);
            this.ComboSelect.ItemsSource = SortingView.SortAlgorithms.Values.ToArray();
            this.ComboSelect.SelectedIndex = 0;
            this.AscendingButton.IsChecked = true;
            this.AddItemsToCanvas();
            this.myView.PrepareRecords();

            this.WholeTimer.Interval = TimeSpan.FromSeconds(0.4);
            this.SwapTimer.Interval = TimeSpan.FromSeconds(0.03);
            this.MoveTimer.Interval = TimeSpan.FromSeconds(0.02);
            this.WholeTimer.Tick += WholeTimer_Tick;
            this.SwapTimer.Tick += SwapTimer_Tick;
            this.MoveTimer.Tick += MoveTimer_Tick;
        }
        #region 事件处理
        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            this.MoveItems();
        }

        private void SwapTimer_Tick(object sender, EventArgs e)
        {
            this.SwapItems();
        }

        private void WholeTimer_Tick(object sender, EventArgs e)
        {
            this.AddRecordToCanvas();
        }

        private void ComboSelect_DropDownClosed(object sender, EventArgs e)
        {
            if (this.ComboSelect.SelectedIndex >= 0 && this.ComboSelect.SelectedIndex < SortingView.SortAlgorithms.Count)
            {
                this.myView.CurAlgorithm = SortingView.SortAlgorithms.Keys.ToArray()[this.ComboSelect.SelectedIndex];
                this.myView.PrepareRecords();
            }
        }

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            var input = this.InputTextBox.Text;
            
            if (String.IsNullOrWhiteSpace(input) || input == InputPrompt)
            {
                MessageBox.Show("输入为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string[] strs = null;

            input = input.Trim();
            strs = input.Split(' ');

            if (strs.Length < SortingView.minNum || strs.Length > SortingView.maxNum)
            {
                MessageBox.Show("为了窗口显示要求，请确保输入数据在15-20个之间！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.myView.Nums = new int[strs.Count()];

            for (int i = 0; i < strs.Count(); ++i)
            {
                this.myView.Nums[i] = Convert.ToInt32(strs[i]);
            }
            Reset();
        }

        private void SetDefaultText()
        {
            this.InputTextBox.Text = InputPrompt;
            this.InputTextBox.Foreground = Brushes.Gray;
        }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.InputTextBox.Text == InputPrompt)
            {
                this.InputTextBox.Text = "";
                this.InputTextBox.Foreground = Brushes.Black;
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(this.InputTextBox.Text))
            {
                SetDefaultText();
            }
        }

        private void AscendingButton_Checked(object sender, RoutedEventArgs e)
        {
            this.myView.AscendingOrder = true;
            this.myView.DescendingOrder = false;
            this.myView.PrepareRecords();
        }

        private void DescendingButton_Checked(object sender, RoutedEventArgs e)
        {
            this.myView.AscendingOrder = false;
            this.myView.DescendingOrder = true;
            this.myView.PrepareRecords();
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.WholeTimer.IsEnabled)
            {
                this.WholeTimer.Start();
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WholeTimer.IsEnabled == true)
            {
                this.WholeTimer.Stop();
            }
            if (this.SwapTimer.IsEnabled == true)
            {
                this.SwapTimer.Stop();
            }
            if (this.MoveTimer.IsEnabled == true)
            {
                this.MoveTimer.Stop();
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
        #endregion
        void ShowMessage(string prompt = null)
        {
            MessageBox.Show(prompt, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public int ItemIndex1 { get; set; }
        public int ItemIndex2 { get; set; }

        public void Reset()
        {
            if (this.WholeTimer.IsEnabled)
                this.WholeTimer.Stop();
            if (this.SwapTimer.IsEnabled)
                this.SwapTimer.Stop();
            if (this.MoveTimer.IsEnabled)
                this.MoveTimer.Stop();
            this.myView.RecordCount = 0;
            this.RemoveItemsFromCanvas();
            this.RemoveItemFromStage(this.myView.Items[0]);
            this.myView.SetItemsParameters();
            this.AddItemsToCanvas();
            this.myView.PrepareRecords();
        }
        #region 记录处理
        public void AddRecordToCanvas()
        {
            if (this.myView.RecordCount >= this.myView.Records.Count)
            {
                if (this.WholeTimer.IsEnabled)
                    this.WholeTimer.Stop();
                if (this.SwapTimer.IsEnabled)
                    this.SwapTimer.Stop();
                if (this.MoveTimer.IsEnabled)
                    this.MoveTimer.Stop();
                ShowMessage("排序完成！");
                return;
            }

            //MessageBox.Show(Convert.ToString(this.myView.Records[this.myView.RecordCount + 1].IsStage) + "  " +
            //    this.myView.Records[this.myView.RecordCount + 1].OperateIndex1 +"  " +
            //    this.myView.Records[this.myView.RecordCount + 1].OperateIndex2);

            this.WholeTimer.Stop();

            var curRecord = this.myView.Records[this.myView.RecordCount];
            this.ItemIndex1 = curRecord.OperateIndex1;
            this.ItemIndex2 = curRecord.OperateIndex2;

            this.myView.SwapCount = -1;
            this.myView.MoveCount = -1;

            if (curRecord.IsSwap)
            {
                this.myView.SwapCount = Math.Abs(this.myView.Items[this.ItemIndex1].RectPosition.X -
                                                 this.myView.Items[this.ItemIndex2].RectPosition.X);
                //MessageBox.Show(Convert.ToString(this.myView.Items[this.ItemIndex1].RectPosition.X) + " "
                //    + Convert.ToString(this.myView.Items[this.ItemIndex2].RectPosition.X) + " " +
                //    this.ItemIndex1 + " " + this.ItemIndex2);
                this.SwapTimer.Start();
            }
            else if (curRecord.IsCompare)
            {
                if (this.ItemIndex1 != 0)
                {
                    this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex1]);
                    this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Compare);
                    this.AddItemToCanvas(this.myView.Items[this.ItemIndex1]);
                }
                else
                {
                    this.RemoveItemFromStage(this.myView.Items[this.ItemIndex1]);
                    this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Compare);
                    this.AddItemToStage(this.myView.Items[this.ItemIndex1]);
                }
                if (this.ItemIndex2!= 0)
                {
                    this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex2]);
                    this.myView.Items[this.ItemIndex2].SetItemState(Item.State.Compare);
                    this.AddItemToCanvas(this.myView.Items[this.ItemIndex2]);
                }
                else
                {
                    this.RemoveItemFromStage(this.myView.Items[this.ItemIndex2]);
                    this.myView.Items[this.ItemIndex2].SetItemState(Item.State.Compare);
                    this.AddItemToStage(this.myView.Items[this.ItemIndex2]);
                }

                ++this.myView.RecordCount;
                this.WholeTimer.Start();
            }
            else if (curRecord.IsSeleted)
            {
                this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex1]);
                this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Selected);
                this.AddItemToCanvas(this.myView.Items[this.ItemIndex1]);
                ++this.myView.RecordCount;
                this.WholeTimer.Start();
            }
            else if (curRecord.IsMerge)
            {
                for (int i = this.ItemIndex1; i <= this.ItemIndex2; ++i)
                {
                    this.RemoveItemFromCanvas(this.myView.Items[i]);
                    this.myView.Items[i].SetItemState(Item.State.Merge);
                    this.AddItemToCanvas(this.myView.Items[i]);
                }
                ++this.myView.RecordCount;
                this.WholeTimer.Start();
            }
            else if (curRecord.IsMergeNormal)
            {
                for (int i = this.ItemIndex1; i <= this.ItemIndex2; ++i)
                {
                    this.RemoveItemFromCanvas(this.myView.Items[i]);
                    this.myView.Items[i].SetItemState(Item.State.Normal);
                    this.AddItemToCanvas(this.myView.Items[i]);
                }
                ++this.myView.RecordCount;
                this.WholeTimer.Start();
            }
            else if (curRecord.IsNormal)
            {
                int i = this.ItemIndex1;
                if (i == 0)
                {
                    this.RemoveItemFromStage(this.myView.Items[i]);
                    this.myView.Items[i].SetItemState(Item.State.Normal);
                    this.AddItemToStage(this.myView.Items[i]);
                }
                else
                {
                    if (this.RemoveItemFromCanvas(this.myView.Items[i]))
                    {
                        this.myView.Items[i].SetItemState(Item.State.Normal);
                        this.AddItemToCanvas(this.myView.Items[i]);
                    }
                }
                i = this.ItemIndex2;
                if (this.RemoveItemFromCanvas(this.myView.Items[i]))
                {
                    this.myView.Items[i].SetItemState(Item.State.Normal);
                    this.AddItemToCanvas(this.myView.Items[i]);
                }

                ++this.myView.RecordCount;
                this.WholeTimer.Start();
            }
            else if (curRecord.IsStage)
            {
                if (this.ItemIndex1 == 0)
                {
                    this.myView.Items[0] = new Item(this.myView.Items[this.ItemIndex2]);
                    this.myView.Items[0].RectPosition = new Point
                    {
                        X = this.myView.WaitLeftMargin,
                        Y = this.myView.Items[this.ItemIndex2].RectPosition.Y
                    };
                    this.myView.Items[0].TextPosition = new Point
                    {
                        X = this.myView.WaitLeftMargin,
                        Y = this.myView.Items[this.ItemIndex2].TextPosition.Y
                    };
                    this.myView.Items[0].SetItemState(Item.State.Stage);

                    this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex2]);
                        
                   
                    this.AddItemToStage(this.myView.Items[0]);
                }
                else if (this.ItemIndex2 == 0)
                {
                    this.RemoveItemFromStage(this.myView.Items[0]);
                    var rectP = this.myView.Items[this.ItemIndex1].RectPosition;
                    var textP = this.myView.Items[this.ItemIndex1].TextPosition;
                    this.myView.Items[this.ItemIndex1] = new Item(this.myView.Items[0]);

                    //MessageBox.Show(Convert.ToString(this.ItemIndex1));

                    this.myView.Items[this.ItemIndex1].RectPosition = new Point
                    {
                        X = rectP.X,
                        Y = this.myView.Items[0].RectPosition.Y
                    };
                    this.myView.Items[this.ItemIndex1].TextPosition = new Point
                    {
                        X = textP.X,
                        Y = this.myView.Items[0].TextPosition.Y
                    };
                    this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Normal);
                    this.AddItemToCanvas(this.myView.Items[this.ItemIndex1]);
                }
                ++this.myView.RecordCount;
                this.WholeTimer.Start();
            }
            else if (curRecord.IsMove)
            {
                if (this.ItemIndex1 != 0 && this.ItemIndex2 != 0)
                {
                    //MessageBox.Show(Convert.ToString(this.ItemIndex1) + " " + Convert.ToString(this.ItemIndex2));

                    this.myView.MoveCount = Math.Abs(this.myView.Items[this.ItemIndex1].RectPosition.X -
                                                     this.myView.Items[this.ItemIndex2].RectPosition.X);
                    
                    this.myView.Items[this.ItemIndex2].RectPosition = this.myView.Items[this.ItemIndex1].RectPosition;
                    this.myView.Items[this.ItemIndex2].TextPosition = this.myView.Items[this.ItemIndex1].TextPosition;

                    this.MoveTimer.Start();
                }
            }
        }

        public void SwapItems()
        {
            if (this.ItemIndex1 == this.ItemIndex2)
            {
                this.SwapTimer.Stop();
                ++this.myView.RecordCount;
                this.WholeTimer.Start();
                return;
            }
                
            if (this.myView.SwapCount > 0)
            {
                int sign = (ItemIndex1 > ItemIndex2 ? -1 : 1);
                var moveWidth = (this.myView.SwapCount > Item.StepWidth ? Item.StepWidth : this.myView.SwapCount);

                this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex1]);

                var rectP = this.myView.Items[this.ItemIndex1].RectPosition;
                var textP = this.myView.Items[this.ItemIndex1].TextPosition;

                this.myView.Items[this.ItemIndex1].RectPosition = new Point
                {
                    X = rectP.X + sign * moveWidth,
                    Y = rectP.Y
                };
                this.myView.Items[this.ItemIndex1].TextPosition = new Point
                {
                    X = textP.X + sign * moveWidth,
                    Y = textP.Y
                };
                this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Swap);
                this.AddItemToCanvas(this.myView.Items[this.ItemIndex1]);


                this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex2]);

                var rectP1 = this.myView.Items[this.ItemIndex2].RectPosition;
                var textP1 = this.myView.Items[this.ItemIndex2].TextPosition;

                this.myView.Items[this.ItemIndex2].RectPosition = new Point
                {
                    X = rectP1.X - sign * moveWidth,
                    Y = rectP1.Y
                };
                this.myView.Items[this.ItemIndex2].TextPosition = new Point
                {
                    X = textP1.X - sign * moveWidth,
                    Y = textP1.Y
                };
                this.myView.Items[this.ItemIndex2].SetItemState(Item.State.Swap);
                this.AddItemToCanvas(this.myView.Items[this.ItemIndex2]);

                this.myView.SwapCount -= Item.StepWidth;
            }
            else
            {
                this.SwapTimer.Stop();

                this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex1]);
                this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Normal);
                this.AddItemToCanvas(this.myView.Items[this.ItemIndex1]);

                this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex2]);
                this.myView.Items[this.ItemIndex2].SetItemState(Item.State.Normal);
                this.AddItemToCanvas(this.myView.Items[this.ItemIndex2]);

                var temp = new Item(this.myView.Items[this.ItemIndex1]);
                this.myView.Items[this.ItemIndex1] = new Item (this.myView.Items[this.ItemIndex2]);

                this.myView.Items[this.ItemIndex2] = new Item(temp);

                ++this.myView.RecordCount;
                this.WholeTimer.Start();
            }
        }

        public void MoveItems()
        {
            if (this.ItemIndex1 == this.ItemIndex2)
            {
                this.MoveTimer.Stop();
                ++this.myView.RecordCount;
                this.WholeTimer.Start();
                return;
            }
            if (this.myView.MoveCount > 0)
            {
                int sign = (ItemIndex1 > ItemIndex2 ? -1 : 1);
                double moveWidth = (this.myView.MoveCount > Item.StepWidth ? Item.StepWidth : this.myView.MoveCount);

                this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex1]);

                var rectP = this.myView.Items[this.ItemIndex1].RectPosition;
                var textP = this.myView.Items[this.ItemIndex1].TextPosition;

                this.myView.Items[this.ItemIndex1].RectPosition = new Point
                {
                    X = rectP.X + sign * moveWidth,
                    Y = rectP.Y
                };
                this.myView.Items[this.ItemIndex1].TextPosition = new Point
                {
                    X = textP.X + sign * moveWidth,
                    Y = textP.Y
                };
                this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Move);
                this.AddItemToCanvas(this.myView.Items[this.ItemIndex1]);
                
                this.myView.MoveCount -= Item.StepWidth;
            }
            else
            {
                this.MoveTimer.Stop();
                this.RemoveItemFromCanvas(this.myView.Items[this.ItemIndex1]);
                this.myView.Items[this.ItemIndex1].SetItemState(Item.State.Normal);
                this.AddItemToCanvas(this.myView.Items[this.ItemIndex1]);

                var rectP = this.myView.Items[this.ItemIndex2].RectPosition;
                var textP = this.myView.Items[this.ItemIndex2].TextPosition;

                this.myView.Items[this.ItemIndex2] = new Item(this.myView.Items[this.ItemIndex1]);

                this.myView.Items[this.ItemIndex1].RectPosition = rectP;
                this.myView.Items[this.ItemIndex1].TextPosition = textP;

                ++this.myView.RecordCount;
                this.WholeTimer.Start();
                
            }
        }
        public void AddItemToStage(Item item)
        {
            this.WaitArea.RegisterName(item.Rectangle.Name, item.Rectangle);
            this.WaitArea.Children.Add(item.Rectangle);
            Canvas.SetLeft(item.Rectangle, item.RectPosition.X);
            Canvas.SetTop(item.Rectangle, item.RectPosition.Y);
            this.WaitArea.RegisterName(item.TextBlock.Name, item.TextBlock);
            this.WaitArea.Children.Add(item.TextBlock);
            Canvas.SetLeft(item.TextBlock, item.TextPosition.X);
            Canvas.SetTop(item.TextBlock, item.TextPosition.Y);
        }
        public void RemoveItemFromStage(Item item = null)
        {
            if (item != null)
            {
                if (this.WaitArea.FindName(item.Rectangle.Name) is Rectangle rect)
                {
                    this.WaitArea.Children.Remove(rect);
                    this.WaitArea.UnregisterName(item.Rectangle.Name);
                }
                if (this.WaitArea.FindName(item.TextBlock.Name) is TextBlock text)
                {
                    this.WaitArea.Children.Remove(text);
                    this.WaitArea.UnregisterName(item.TextBlock.Name);
                }
            }
            else
            {
                this.WaitArea.Children.Clear();
            }
        }
        public void AddItemToCanvas(Item item)
        {
            this.DemoArea.Children.Add(item.Rectangle);
            this.DemoArea.RegisterName(item.Rectangle.Name, item.Rectangle);
            Canvas.SetLeft(item.Rectangle, item.RectPosition.X);
            Canvas.SetTop(item.Rectangle, item.RectPosition.Y);

            this.DemoArea.Children.Add(item.TextBlock);
            this.DemoArea.RegisterName(item.TextBlock.Name, item.TextBlock);
            Canvas.SetLeft(item.TextBlock, item.TextPosition.X);
            Canvas.SetTop(item.TextBlock, item.TextPosition.Y);
        }
        public void AddItemsToCanvas()
        {
            for (int i = 1; i < this.myView.Items.Count; ++i)
            {
                this.AddItemToCanvas(this.myView.Items[i]);
            }
        }
        public bool RemoveItemFromCanvas(Item item)
        {
            if (this.DemoArea.FindName(item.Rectangle.Name) is Rectangle rect && (rect.Parent as Canvas == this.DemoArea))
            {
                this.DemoArea.Children.Remove(rect);
                this.DemoArea.UnregisterName(item.Rectangle.Name);
            }
            else
            {
                return false;
            }
            if (this.DemoArea.FindName(item.TextBlock.Name) is TextBlock text && (text.Parent as Canvas == this.DemoArea))
            {
                this.DemoArea.Children.Remove(text);
                this.DemoArea.UnregisterName(item.TextBlock.Name);
            }
            else
            {
                return false;
            }
            return true;
        }
        public void RemoveItemsFromCanvas()
        {
            for (int i = 1; i < this.myView.Items.Count; ++i)
            {
                this.RemoveItemFromCanvas(this.myView.Items[i]);
            }
        }
        #endregion
    }

    public class SortingView
    {
        #region 视图参数定义
        static Random random = new Random();
        public const int minNum = 15;
        public const int maxNum = 20;
        const int minValue = -10;
        const int maxValue = 20;
        public static Dictionary<string, string> SortAlgorithms = new Dictionary<string, string>(){
                                                                  { "InsertSort", "插入排序" }, {"BinaryInsertSort", "折半插入排序" },
                                                                  { "ShellSort", "希尔排序" },  {"BubbleSort", "冒泡排序" },
                                                                  { "QuickSort", "快速排序" },  {"SelectSort", "选择排序" },
                                                                  { "HeapSort", "堆排序" },     {"MergeSort", "归并排序" } };
        public string CurAlgorithm { get; set; }
        public int[] Nums = null;

        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public double SubWindowWidth { get; set; }
        public double LeftMargin { get; set; }
        public double TopMargin { get; set; }
        public double WaitLeftMargin { get; set; }

        public bool AscendingOrder { get; set; }
        public bool DescendingOrder { get; set; }

        public int RecordCount { get; set; }
        public double SwapCount { get; set; }
        public double MoveCount { get; set; }

        private List<Item> items;
        public List<Item> Items
        {
            get
            {
                if (items == null)
                    items = new List<Item>();
                return items;
            }
        }

        private List<Record> records;
        public List<Record> Records
        {
            get
            {
                if (records == null)
                    records = new List<Record>();
                return records;
            }
        }
        #endregion
        public SortingView(double width, double height, double waitWidth)
        {
            this.CurAlgorithm = SortingView.SortAlgorithms.Keys.ToArray()[0];
            this.WindowWidth = width;
            this.WindowHeight = height;
            this.SubWindowWidth = waitWidth;
            this.AscendingOrder = true;
            this.DescendingOrder = !this.AscendingOrder;
            this.SetItemsParameters();
        }

        #region 设置Items的各项参数
        public void SetItemsParameters()
        {
            this.SetItemsValue();
            this.SetItemsColor();
            this.SetItemsRectangle();
            this.SetItemsPosition();
            this.SetItemsName();
        }
        public void SetItemsValue()
        {
            this.Items.Clear();
            var size = random.Next(minNum, maxNum);
            if (this.Nums == null)
            {
                
                this.Items.Add(new Item(int.MinValue));
                for (int i = 0; i < size; ++i)
                {
                    this.Items.Add(new Item(random.Next(minValue, maxValue)));
                }
            }
            else
            {
                this.Items.Add(new Item(int.MinValue));
                for (int i = 0; i < this.Nums.Length; ++i)
                {
                    this.Items.Add(new Item(this.Nums[i]));
                }
            }
        }
        public void SetItemsColor()
        {
            HashSet<int> valueSet = new HashSet<int>();
            for (int i = 1; i < this.Items.Count; ++i)
            {
                if (valueSet.Add(this.Items[i].Value) == false)
                {
                    this.Items[i].OriginBrush = new SolidColorBrush(Color.FromRgb(
                                                    (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)));
                    this.Items[i].Brush = new SolidColorBrush(this.Items[i].OriginBrush.Color);
                }
            }
        }

        public void SetItemsRectangle()
        {
            var size = this.items.Count;
            var plusMin = int.MaxValue;
            var plusMax = 0;
            var negMin = -1;
            var negMax = int.MinValue;

            for (int i = 1; i < size; ++i)
            {
                if (this.Items[i].Value >= 0)
                {
                    plusMin = Math.Min(plusMin, this.Items[i].Value);
                    plusMax = Math.Max(plusMax, this.Items[i].Value);
                }
                else
                {
                    negMin = Math.Min(negMin, this.Items[i].Value);
                    negMax = Math.Max(negMax, this.Items[i].Value);
                }

            }
            negMin = -negMin;
            negMax = -negMax;

            negMin ^= negMax;
            negMax ^= negMin;
            negMin ^= negMax;

            this.Items[0].Rectangle = new Rectangle();

            for (int i = 1; i < size; ++i)
            {
                double height;
                if (this.Items[i].Value >= 0)
                {
                    height = Convert.ToDouble(this.Items[i].Value - plusMin + 1) / (plusMax - plusMin + 1) * Item.BaseHeight;
                }
                else
                {
                    height = Convert.ToDouble(Math.Abs(this.Items[i].Value) - negMin + 1) / (negMax - negMin + 1) * Item.BaseHeight;
                }
                this.Items[i].Rectangle = new Rectangle
                {
                    Height = height,
                    Width = Item.Width,
                    Fill = this.Items[i].Brush
                };
            }
        }

        public void SetItemsPosition()
        {
            var size = this.items.Count;

            this.LeftMargin = (this.WindowWidth - Item.Width * size - Item.GapWidth * (size - 1)) / 2;
            this.TopMargin = this.WindowHeight / 2;
            this.WaitLeftMargin = (this.SubWindowWidth - Item.Width) / 2;

            if (size > 1)
            {
                this.Items[1].RectPosition = new Point(this.LeftMargin,
                                                       this.TopMargin - (this.Items[1].Value >= 0 ? this.Items[1].Rectangle.Height : 0));
                this.Items[1].TextPosition = new Point(this.Items[1].RectPosition.X, this.Items[1].RectPosition.Y + this.Items[1].Rectangle.Height);
            }
            for (int i = 2; i < size; ++i)
            {
                this.Items[i].RectPosition = new Point(this.Items[i - 1].RectPosition.X + Item.Width + Item.GapWidth,
                                                       this.TopMargin - (this.Items[i].Value >= 0 ? this.Items[i].Rectangle.Height : 0));
                this.Items[i].TextPosition = new Point(this.Items[i].RectPosition.X, this.Items[i].RectPosition.Y + this.Items[i].Rectangle.Height);
            }
        }
        public void SetItemsName()
        {
            for (int i = 1; i < this.items.Count; ++i)
            {
                this.Items[i].Rectangle.Name = "Rect" + Convert.ToString(i);
                this.Items[i].TextBlock.Name = "Text" + Convert.ToString(i);
            }
        }
        #endregion

        #region 定义比较方法
        public bool Compare(int a, int b, bool strict = true)
        {
            if (this.AscendingOrder)
            {
                if (strict)
                {
                    return a < b;
                }
                else
                {
                    return a <= b;
                }
            }
            else
            {
                if (strict)
                {
                    return a > b;
                }
                else
                {
                    return a >= b;
                }
            }
        }
        #endregion

        public void PrepareRecords()
        {
            this.Records.Clear();
            Type type = typeof(SortingView);
            MethodInfo method = type.GetMethod(this.CurAlgorithm, BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                return;
            }
            else
            {
                method.Invoke(this, null);
            }
        }

        private int[] CopyValues()
        {
            int[] nums = new int[this.Items.Count];
            nums[0] = -1;
            for (int i = 1; i < this.Items.Count; ++i)
            {
                nums[i] = this.Items[i].Value;
            }
            return nums;
        }
        #region 排序算法
        public void InsertSort()
        {
            var nums = CopyValues();
            for (int i = 2, j = 0; i < nums.Length; ++i)
            {
                this.Records.Add(new Record(Record.Operate.Compare, i, i - 1));
                this.Records.Add(new Record(Record.Operate.Normal, i - 1, i));
                if (Compare(nums[i], nums[i - 1]))
                {
                    this.Records.Add(new Record(Record.Operate.Stage, 0, i));
                    nums[0] = nums[i];
                    this.Records.Add(new Record(Record.Operate.Move, i - 1, i));
                    nums[i] = nums[i - 1];
                    this.Records.Add(new Record(Record.Operate.Compare, 0, i - 2));
                    this.Records.Add(new Record(Record.Operate.Normal, 0, i - 2));
                    for (j = i - 2; Compare(nums[0], nums[j]); --j)
                    {
                        this.Records.Add(new Record(Record.Operate.Move, j, j + 1));
                        nums[j + 1] = nums[j];
                        this.Records.Add(new Record(Record.Operate.Compare, 0, j - 1));
                        this.Records.Add(new Record(Record.Operate.Normal, 0, j - 1));
                    }
                    this.Records.Add(new Record(Record.Operate.Stage, j + 1, 0));
                    nums[j + 1] = nums[0];
                }
            }
        }

        public void BinaryInsertSort()
        {
            var nums = CopyValues();
            int low, high, mid;
            for (int i = 2, j = 0; i < nums.Length; ++i)
            {
                this.Records.Add(new Record(Record.Operate.Stage, 0, i));
                nums[0] = nums[i];
                low = 1;
                high = i - 1;
                while (low <= high)
                {
                    mid = low + (high - low) / 2;
                    this.Records.Add(new Record(Record.Operate.Compare, 0, mid));
                    if (Compare(nums[0], nums[mid]))
                        high = mid - 1;
                    else
                        low = mid + 1;
                    this.Records.Add(new Record(Record.Operate.Normal, 0, mid));
                }
                for (j = i - 1; j >= high + 1; --j)
                {
                    this.Records.Add(new Record(Record.Operate.Move, j, j + 1));
                    nums[j + 1] = nums[j];
                }
                this.Records.Add(new Record(Record.Operate.Stage, high + 1, 0));
                nums[high + 1] = nums[0];
            }
        }
        public void ShellInsert(int[] nums, int delta)
        {
            for (int i = delta + 1, j = 0; i < nums.Length; ++i)
            {
                this.Records.Add(new Record(Record.Operate.Compare, i, i - delta));
                this.Records.Add(new Record(Record.Operate.Normal, i, i - delta));
                if (Compare(nums[i], nums[i - delta]))
                {
                    this.Records.Add(new Record(Record.Operate.Stage, 0, i));
                    nums[0] = nums[i];
                    this.Records.Add(new Record(Record.Operate.Compare, 0, i - delta));
                    this.Records.Add(new Record(Record.Operate.Normal, 0, i - delta));
                    for (j = i - delta; j > 0 && Compare(nums[0], nums[j]); j -= delta)
                    {
                        this.Records.Add(new Record(Record.Operate.Move, j, j + delta));
                        nums[j + delta] = nums[j];
                        if (j - delta > 0)
                        {
                            this.Records.Add(new Record(Record.Operate.Compare, 0, j - delta));
                            this.Records.Add(new Record(Record.Operate.Normal, 0, j - delta));
                        }
                    }
                    this.Records.Add(new Record(Record.Operate.Stage, j + delta, 0));
                    nums[j + delta] = nums[0];
                }
            }
        }
        public void ShellSort()
        {
            var nums = CopyValues();
            var delta = ShellDelta(nums.Length - 1);
            foreach (var del in delta)
            {
                ShellInsert(nums, del);
            }

        }
        public void BubbleSort()
        {
            var nums = CopyValues();
            bool SwapFlag = true;
            for (int i = nums.Length - 1, j = 0; SwapFlag && i > 2; --i)
            {
                SwapFlag = false;
                for (j = 1; j < i; ++j)
                {
                    this.Records.Add(new Record(Record.Operate.Compare, j, j + 1));
                    this.Records.Add(new Record(Record.Operate.Normal, j, j + 1));
                    if (Compare(nums[j + 1], nums[j]))
                    {
                        this.Records.Add(new Record(Record.Operate.Swap, j, j + 1));
                        nums[j + 1] ^= nums[j];
                        nums[j] ^= nums[j + 1];
                        nums[j + 1] ^= nums[j];
                        SwapFlag = true;
                    }
                }
            }
        }
        public int Partition(int[] nums, int left, int right)
        {
            this.Records.Add(new Record(Record.Operate.Stage, 0, left));
            nums[0] = nums[left];
            int pivotValue = nums[left];
            while (left < right)
            {
                if (left < right)
                {
                    this.Records.Add(new Record(Record.Operate.Compare, 0, right));
                    this.Records.Add(new Record(Record.Operate.Normal, 0, right));
                }
                
                while ((left < right) && (!Compare(nums[right], pivotValue)))
                {
                    --right;
                    if (left < right)
                    {
                        this.Records.Add(new Record(Record.Operate.Compare, 0, right));
                        this.Records.Add(new Record(Record.Operate.Normal, 0, right));
                    }
                }
                this.Records.Add(new Record(Record.Operate.Move, right, left));
                nums[left] = nums[right];

                if (left < right)
                {
                    this.Records.Add(new Record(Record.Operate.Compare, 0, left));
                    this.Records.Add(new Record(Record.Operate.Normal, 0, left));
                }
                while ((left < right) && Compare(nums[left], pivotValue, false))
                {
                    ++left;
                    if (left < right)
                    {
                        this.Records.Add(new Record(Record.Operate.Compare, 0, left));
                        this.Records.Add(new Record(Record.Operate.Normal, 0, left));
                    }
                }
                this.Records.Add(new Record(Record.Operate.Move, left, right));
                nums[right] = nums[left];
            }

            this.Records.Add(new Record(Record.Operate.Stage, left, 0));
            nums[left] = nums[0];
            return left;
        }
        public void QSort(int[] nums, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(nums, left, right);
                QSort(nums, left, pivot - 1);
                QSort(nums, pivot + 1, right);
            }
        }
        public void QuickSort()
        {
            var nums = CopyValues();
            QSort(nums, 1, nums.Length - 1);
        }

        public void SelectSort()
        {
            var nums = CopyValues();
            for (int i = 1, j = 0; i < nums.Length - 1; ++i)
            {
                int k = i;
                for (j = i; j < nums.Length; ++j)
                {
                    this.Records.Add(new Record(Record.Operate.Compare, k, j));
                    this.Records.Add(new Record(Record.Operate.Normal, k, j));
                    if (Compare(nums[j], nums[k]))
                    {
                        k = j;
                    }
                }
                if (k != i)
                {
                    this.Records.Add(new Record(Record.Operate.Swap, i, k));
                    nums[i] ^= nums[k];
                    nums[k] ^= nums[i];
                    nums[i] ^= nums[k];
                }
            }
        }
        public void HeapAdjust(int[] nums, int s, int m)
        {
            this.Records.Add(new Record(Record.Operate.Stage, 0, s));
            int rc = nums[s];
            for (int j = 2 * s; j <= m; j *= 2)
            {
                if (j < m)
                {
                    this.Records.Add(new Record(Record.Operate.Compare, j, j + 1));
                    this.Records.Add(new Record(Record.Operate.Normal, j, j + 1));
                }
                    
                if (j < m && Compare(nums[j], nums[j + 1]))
                    ++j;
                this.Records.Add(new Record(Record.Operate.Compare, 0, j));
                this.Records.Add(new Record(Record.Operate.Normal, 0, j));
                if (!Compare(rc, nums[j]))
                    break;
                this.Records.Add(new Record(Record.Operate.Move, j, s));
                nums[s] = nums[j];
                s = j;
            }
            this.Records.Add(new Record(Record.Operate.Stage, s, 0));
            nums[s] = rc;
        }
        public void HeapSort()
        {
            var nums = CopyValues();
            for (int i = (nums.Length - 1) / 2; i > 0; --i)
                HeapAdjust(nums, i, nums.Length - 1);
            for (int i = nums.Length - 1; i > 1; --i)
            {
                this.Records.Add(new Record(Record.Operate.Swap, 1, i));
                nums[i] ^= nums[1];
                nums[1] ^= nums[i];
                nums[i] ^= nums[1];

                HeapAdjust(nums, 1, i - 1);
            }
        }
        public int BinarySearch(int[] nums, int target, int left, int right)
        {
            int mid = 0;
            while (left <= right)
            {
                mid = left + (right - left) / 2;
                if (nums[mid] == target)
                    return mid;
                else if (Compare(target, nums[mid]))
                {
                    right = mid - 1;
                }
                else
                    left = mid + 1;
            }
            if (Compare(target, nums[mid]))
            {
                return right;
            }
            else
            {
                return mid;
            }
        }
        void ShiftRight(int[] nums, int left, int right, int k)
        {
            for (int i = 0; i < k; ++i)
            {
                this.Records.Add(new Record(Record.Operate.Stage, 0, right));
                int temp = nums[right];
                for (int j = right; j > left; --j)
                {
                    this.Records.Add(new Record(Record.Operate.Move, j - 1, j));
                    nums[j] = nums[j - 1];
                }
                this.Records.Add(new Record(Record.Operate.Stage, left, 0));
                nums[left] = temp;
            }
        }
        public void Merge(int[] nums, int low, int mid, int high)
        {
            this.Records.Add(new Record(Record.Operate.Merge, low, high));
            this.Records.Add(new Record(Record.Operate.MergeNormal, low, high));
            mid += 1;
            while (low < mid && mid <= high)
            {
                int p = BinarySearch(nums, nums[low], mid, high);
                ShiftRight(nums, low, p, p - mid + 1);
                mid = p + 1;
                low += p - mid + 2;
            }
        }
        public void MSort(int[] nums, int low, int high)
        {
            if (low == high)
                return;
            else
            {
                int mid = low + (high - low) / 2;
                MSort(nums, low, mid);
                MSort(nums, mid + 1, high);
                Merge(nums, low, mid, high);
            }
        }
        public void MergeSort()
        {
            var nums = CopyValues();

            MSort(nums, 1, nums.Length - 1);
        }

        public int[] ShellDelta(int n)
        {
            List<int> delta = new List<int>();
            while (true)
            {
                if (n > 2)
                    delta.Add((n /= 2));
                else
                {
                    delta.Add(1);
                    break;
                }
            }
            return delta.ToArray();
        }
        #endregion
    }
    #region Item类定义
    public class Item
    {
        public enum State { Swap, Compare, Selected, Merge, Stage, Move, Normal };
        public static SolidColorBrush[] StateBrush = new SolidColorBrush[] {
                                        Brushes.GreenYellow, Brushes.Red, Brushes.Green, Brushes.Blue,
                                        Brushes.Aquamarine, Brushes.GreenYellow, Brushes.Aquamarine };
        public static Color TextColor = new Color()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 255
        };

        /* 每个Item对应一个矩形 */
        public const double Width = 35;       // 矩形对应宽，已确定
        public const double BaseHeight = 150;  // 矩形对应基础高度(即值为零时为该值，其他相应加基础高度)
        public const double GapWidth = Width / 3 * 2; // 每个矩形间的距离
        public const double StepWidth = Width / 5;    // 动画演示时每步移动的长度
        public Rectangle Rectangle { get; set; }
        public TextBlock TextBlock { get; set; }
        public Point RectPosition { get; set; }
        public Point TextPosition { get; set; }
        public SolidColorBrush Brush { get; set; }
        public SolidColorBrush OriginBrush { get; set; }
        public int Value { get; set; }

        public Item() { }

        public Item(Item item)
        {
            this.Rectangle = new Rectangle()
            {
                Width = item.Rectangle.Width,
                Height = item.Rectangle.Height,
                Fill = item.Rectangle.Fill,
                Name = item.Rectangle.Name
            };
            this.TextBlock = new TextBlock()
            {
                Text = item.TextBlock.Text,
                FontSize = item.TextBlock.FontSize,
                TextAlignment = item.TextBlock.TextAlignment,
                Width = Item.Width,
                Name = item.TextBlock.Name
            };
            this.RectPosition = item.RectPosition;
            this.TextPosition = item.TextPosition;
            this.Brush = new SolidColorBrush(item.Brush.Color);
            this.OriginBrush = new SolidColorBrush(item.OriginBrush.Color);
            this.Value = item.Value;
        }
        public Item(int value)
        {
            this.Value = value;
            this.Brush = StateBrush[Convert.ToInt32(State.Normal)];
            this.OriginBrush = new SolidColorBrush(this.Brush.Color);

            this.TextBlock = new TextBlock
            {
                Text = Convert.ToString(this.Value),
                FontSize = 15,
                TextAlignment = TextAlignment.Center,
                Width = Item.Width
            };
        }

        public void SetItemState(State curState)
        {
            this.Brush = StateBrush[Convert.ToInt32(curState)];
            if (curState == State.Normal)
                this.Brush = new SolidColorBrush(this.OriginBrush.Color);
            this.Rectangle.Fill = this.Brush;
        }
        public static bool operator <(Item a, Item b) => (a.Value < b.Value);
        public static bool operator >(Item a, Item b) => (a.Value > b.Value);
        public static bool operator >=(Item a, Item b) => (a.Value >= b.Value);
        public static bool operator <=(Item a, Item b) => (a.Value <= b.Value);        
    }
    #endregion

    #region 动画演示的记录项定义
    public class Record
    {
        public enum Operate { Swap, Compare, Merge, MergeNormal, Select, Stage, Move, Normal };
        public bool IsSwap { get; set; }        // 交换 item 标记 双索引
        public bool IsCompare { get; set; }     // 比较 item 标记 双索引
        public bool IsMerge { get; set; }       // 合并 item 标记 双索引，表示范围
        public bool IsSeleted { get; set; }     // 选择标记       单索引 默认为 OperateIndex1
        public bool IsNormal { get; set; }      // 恢复正常状态    单索引 默认为 OperateIndex1
        public bool IsMergeNormal { get; set; }
        public bool IsStage { get; set; }       // 暂存
        public bool IsMove { get; set; }        // 移动
        public int OperateIndex1 { get; set; }  // Item 索引
        public int OperateIndex2 { get; set; }  // Item 索引

        public Record()
        {
            this.IsSwap = false;
            this.IsCompare = false;
            this.IsSeleted = false;
            this.IsNormal = false;
            this.OperateIndex1 = -1;
            this.OperateIndex2 = -1;
        }

        public Record(Operate option, int Index1, int Index2 = -1)
        {
            this.IsSwap = (option == Operate.Swap);
            this.IsCompare = (option == Operate.Compare);
            this.IsMerge = (option == Operate.Merge);
            this.IsSeleted = (option == Operate.Select);
            this.IsNormal = (option == Operate.Normal);
            this.IsStage = (option == Operate.Stage);
            this.IsMove = (option == Operate.Move);
            this.IsMergeNormal = (option == Operate.MergeNormal);
            this.OperateIndex1 = Index1;
            this.OperateIndex2 = Index2;
        }
    }
    #endregion
}
