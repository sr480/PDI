using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;

namespace ChartControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ChartControl : UserControl
    {
        private Line _hLine;
        private Line _limitLine;

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ChartControl), new UIPropertyMetadata("Title"));
        private Line _vLine;
        public static readonly DependencyProperty YLableProperty =
            DependencyProperty.Register("YLable", typeof(string), typeof(ChartControl), new UIPropertyMetadata("Y Lable"));
        public static readonly DependencyProperty XLableProperty =
            DependencyProperty.Register("XLable", typeof(string), typeof(ChartControl), new UIPropertyMetadata("X Lable"));
        public static readonly DependencyProperty XMaximumProperty =
            DependencyProperty.Register("XMaximum", typeof(double), typeof(ChartControl), new UIPropertyMetadata(100.0));
        public static readonly DependencyProperty XMinimumProperty =
            DependencyProperty.Register("XMinimum", typeof(double), typeof(ChartControl), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty YMaximumProperty =
            DependencyProperty.Register("YMaximum", typeof(double), typeof(ChartControl), new UIPropertyMetadata(100.0));
        public static readonly DependencyProperty YMinimumProperty =
            DependencyProperty.Register("YMinimum", typeof(double), typeof(ChartControl), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty XGridStepProperty =
            DependencyProperty.Register("XGridStep", typeof(double), typeof(ChartControl), new UIPropertyMetadata(10.0));
        public static readonly DependencyProperty YGridStepProperty =
            DependencyProperty.Register("YGridStep", typeof(double), typeof(ChartControl), new UIPropertyMetadata(10.0));
        public static readonly DependencyProperty DataMemberProperty =
            DependencyProperty.Register("DataMember", typeof(string), typeof(ChartControl));
        public static readonly DependencyProperty ValueMembersProperty =
            DependencyProperty.Register("ValueMembers", typeof(ObservableCollection<ValueMemberDefinition>), typeof(ChartControl));
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(object), typeof(ChartControl));
        public static readonly DependencyProperty AutoCalculateAxisLimitsProperty =
            DependencyProperty.Register("AutoCalculateAxisLimits", typeof(bool), typeof(ChartControl), new UIPropertyMetadata(true));
        public static readonly DependencyProperty LimitLineProperty =
            DependencyProperty.Register("LimitLine", typeof(double), typeof(ChartControl), new UIPropertyMetadata(double.NaN));

        bool _supressredraw = false;

        double _height;
        double _width;

        double _mouse_X;
        double _mouse_Y;

        Dictionary<ValueMemberDefinition, TextBlock> _textBoxesForValueMembers;
        TextBlock _XValueTextBox;

        public bool AutoCalculateAxisLimits
        {
            get { return (bool)GetValue(AutoCalculateAxisLimitsProperty); }
            set { SetValue(AutoCalculateAxisLimitsProperty, value); }
        }
        public double XMaximum
        {
            get { return (double)GetValue(XMaximumProperty); }
            set { SetValue(XMaximumProperty, value); }
        }
        public double XMinimum
        {
            get { return (double)GetValue(XMinimumProperty); }
            set { SetValue(XMinimumProperty, value); }
        }
        public double YMaximum
        {
            get { return (double)GetValue(YMaximumProperty); }
            set { SetValue(YMaximumProperty, value); }
        }
        public double YMinimum
        {
            get { return (double)GetValue(YMinimumProperty); }
            set { SetValue(YMinimumProperty, value); }
        }
        public double XGridStep
        {
            get { return (double)GetValue(XGridStepProperty); }
            set { SetValue(XGridStepProperty, value); }
        }
        public double YGridStep
        {
            get { return (double)GetValue(YGridStepProperty); }
            set { SetValue(YGridStepProperty, value); }
        }
        public string YLable
        {
            get { return (string)GetValue(YLableProperty); }
            set { SetValue(YLableProperty, value); }
        }
        public string XLable
        {
            get { return (string)GetValue(XLableProperty); }
            set { SetValue(XLableProperty, value); }
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public string DataMember
        {
            get { return (string)GetValue(DataMemberProperty); }
            set { SetValue(DataMemberProperty, value); }
        }
        public ObservableCollection<ValueMemberDefinition> ValueMembers
        {
            get { return (ObservableCollection<ValueMemberDefinition>)GetValue(ValueMembersProperty); }
            set { SetValue(ValueMembersProperty, value); }
        }
        public object DataSource
        {
            get { return GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        public double LimitLine
        {
            get { return (double)GetValue(LimitLineProperty); }
            set { SetValue(LimitLineProperty, value); }
        }

        private List<RawValueInfo> rawData;

        public ChartControl()
        {
            _textBoxesForValueMembers = new Dictionary<ValueMemberDefinition, TextBlock>();
            ValueMembers = new ObservableCollection<ValueMemberDefinition>();
            InitializeComponent();
            plotGrid.SizeChanged += new SizeChangedEventHandler(plotGrid_SizeChanged);
            plot.SizeChanged += new SizeChangedEventHandler(plot_SizeChanged);

            xAxisValues.SizeChanged += new SizeChangedEventHandler(xAxisValues_SizeChanged);
            yAxisValues.SizeChanged += new SizeChangedEventHandler(yAxisValues_SizeChanged);
        }

        private void ReadRawData()
        {
            //var start = DateTime.Now;
            rawData = new List<RawValueInfo>();
            if (DataSource == null)
                return;

            foreach (var item in (IEnumerable)DataSource)
            {
                try
                {
                    double data = Convert.ToDouble(item.GetType().GetProperty(DataMember).GetValue(item, null));
                    foreach (var valMember in ValueMembers)
                    {
                        try
                        {
                            double value = Convert.ToDouble(item.GetType().GetProperty(valMember.Member).GetValue(item, null));
                            rawData.Add(new RawValueInfo(data, value, valMember));
                        }
                        catch { }
                    }
                }
                catch { }
            }
            //Console.WriteLine("ReadData: {0} ms", (DateTime.Now - start).Milliseconds);
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DataSourceProperty)
            {
                if (e.OldValue is INotifyCollectionChanged)
                    (e.OldValue as INotifyCollectionChanged).CollectionChanged -= DataSourceChanged;
                if (e.NewValue is INotifyCollectionChanged)
                    (e.NewValue as INotifyCollectionChanged).CollectionChanged += DataSourceChanged;
                if (e.NewValue == null)
                    return;

                ReadData();
            }
            if (e.Property == XMaximumProperty |
                e.Property == YMaximumProperty |
                e.Property == XMinimumProperty |
                e.Property == YMinimumProperty |
                e.Property == XGridStepProperty |
                e.Property == YGridStepProperty |
                e.Property == LimitLineProperty)
                RedrawAll();
        }

        void DataSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ReadData();
        }
        private void ReadData()
        {
            _supressredraw = true;
            ReadRawData();
            if (AutoCalculateAxisLimits)
                CalculateAxisLimits();
            _supressredraw = false;
            if (IsVisible)
                RedrawAll();
        }
        public void CalculateAxisLimits()
        {
            if (rawData == null | rawData.Count == 0) return;

            double maxValue = rawData.Max(r => r.Value);
            double minValue = rawData.Min(r => r.Value);
            double maxData = rawData.Max(r => r.Data);
            double minData = rawData.Min(r => r.Data);

            double xLength = Math.Abs(maxData - minData);
            double yLength = Math.Abs(maxValue - minValue);
            
            XMinimum = minData;
            XMaximum = maxData;
            YMinimum = minValue;
            YMaximum = maxValue;

            XGridStep = xLength / 10;
            YGridStep = yLength / 10;
        }
        public void RedrawAll()
        {
            if (_supressredraw)
                return;
            //var start = DateTime.Now;
            RedrawPlot(plot.ActualWidth, plot.ActualHeight);
            //Console.WriteLine("RedrawPlot: {0} ms", (DateTime.Now - start).Milliseconds);

            //start = DateTime.Now;
            RedrawYAxis(yAxisValues.ActualHeight);
            //Console.WriteLine("RedrawYAxis: {0} ms", (DateTime.Now - start).Milliseconds);

            //start = DateTime.Now;
            RedrawXAxis(xAxisValues.ActualWidth);
            //Console.WriteLine("RedrawXAxis: {0} ms", (DateTime.Now - start).Milliseconds);

            //start = DateTime.Now;
            RedrawGrid(plotGrid.ActualWidth, plotGrid.ActualHeight);
            //Console.WriteLine("RedrawGrid: {0} ms", (DateTime.Now - start).Milliseconds);

            DrawLimitLine(plotGrid.ActualWidth, plotGrid.ActualHeight);

            //start = DateTime.Now;
            DrawMouseCross();
            //Console.WriteLine("DrawMouseCross: {0} ms", (DateTime.Now - start).Milliseconds);
        }

        private void DrawLimitLine(double width, double height)
        {
            //Draw LimitLine
            if (!double.IsNaN(LimitLine))
            {
                double yTransform = height / (YMaximum - YMinimum);
                if (_limitLine == null)
                {
                    _limitLine = new Line() { Stroke = System.Windows.Media.Brushes.LimeGreen };
                    limitLineGrid.Children.Add(_limitLine);
                }
                _limitLine.X1 = 0;
                _limitLine.Y1 = height - LimitLine * yTransform;
                _limitLine.X2 = width;
                _limitLine.Y2 = height - LimitLine * yTransform;
            }
        }

        //Redraw plot
        private void RedrawPlot(double width, double height)
        {
            plot.Children.Clear();
            if (rawData == null || rawData.Count == 0)
                return;

            double yTransform = height / (YMaximum - YMinimum);
            if (double.IsInfinity(yTransform))
                yTransform = 1;

            double xTransform = width / (XMaximum - XMinimum);
            if (double.IsInfinity(xTransform))
                xTransform = 1;

            var groupedData = rawData.GroupBy(g => g.ValueMember);
            foreach (var grp in groupedData)
            {
                var sortedData = grp.OrderBy(o => o.Data).ToList();
                for (int i = 1; i < sortedData.Count(); i++)
                {
                    plot.Children.Add(new Line()
                        {
                            Stroke = grp.Key.Color,
                            X1 = xTransform * (sortedData[i - 1].Data - XMinimum),
                            Y1 = height - yTransform * (sortedData[i - 1].Value - YMinimum),
                            X2 = xTransform * (sortedData[i].Data - XMinimum),
                            Y2 = height - yTransform * (sortedData[i].Value - YMinimum)
                        });
                }
            }
        }
        void plot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawPlot(e.NewSize.Width, e.NewSize.Height);
        }

        //Redraw vertical axis
        private void RedrawYAxis(double height)
        {
            //if (_height == height)
            //    return;

            _height = height;

            if (YMaximum == YMinimum)
                YMaximum = YMinimum + 1;
            yAxisValues.Children.Clear();
            double yTransform = height / (YMaximum - YMinimum);
            int hCount = (int)((YMaximum - YMinimum) / YGridStep);
            for (int h = 0; h <= hCount; h++)
            {
                yAxisValues.Children.Add(new TextBlock()
                {
                    Text = (YMinimum + YGridStep * h).ToString("F1"),
                    Margin = new Thickness(0, height - h * YGridStep * yTransform, 0, 1)
                });
            }

            //Remove overlapping values
            //for (int i = 0; i < yAxisValues.Children.Count; i++)
            //{
            //    for (int j = i + 1; j < yAxisValues.Children.Count; )
            //    {
            //        var iCh = (TextBlock)yAxisValues.Children[i];
            //        var jCh = (TextBlock)yAxisValues.Children[j];
            //        iCh.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //        if (iCh.DesiredSize.Height > jCh.Margin.Top)
            //            yAxisValues.Children.RemoveAt(j);
            //        else
            //            break;
            //    }
            //}
        }
        void yAxisValues_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawYAxis(e.NewSize.Height);
        }

        //Redraw horisontal axis
        private void RedrawXAxis(double width)
        {
            //if (_width == width)
            //    return;
            _width = width;

            if (XMaximum == XMinimum)
                XMaximum = XMinimum + 1;
            xAxisValues.Children.Clear();
            double xTransform = width / (XMaximum - XMinimum);

            int vCount = (int)((XMaximum - XMinimum) / XGridStep);
            for (int v = 0; v <= vCount; v++)
            {
                xAxisValues.Children.Add(new TextBlock()
                {
                    Text = (XMinimum + XGridStep * v).ToString("F1"),
                    Margin = new Thickness(v * XGridStep * xTransform, 1, 0, 1)
                });
            }

            //Remove overlapping values
            for (int i = 0; i < xAxisValues.Children.Count; i++)
            {
                for (int j = i + 1; j < xAxisValues.Children.Count; )
                {
                    var iCh = (TextBlock)xAxisValues.Children[i];
                    var jCh = (TextBlock)xAxisValues.Children[j];
                    iCh.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    if (iCh.DesiredSize.Width > jCh.Margin.Left)
                        xAxisValues.Children.RemoveAt(j);
                    else
                        break;
                }
            }
        }
        void xAxisValues_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawXAxis(e.NewSize.Width);
        }

        //Redraw grid
        private void RedrawGrid(double width, double height)
        {
            if (_width == width && _height == height)
                return;
            _width = width;
            _height = height;

            plotGrid.Children.Clear();

            var gridStroke = Brushes.LightGray;
            var axisStroke = Brushes.Black;

            double xTransform = width / (XMaximum - XMinimum);
            double yTransform = height / (YMaximum - YMinimum);
            //Draw vertical grid
            int vCount = (int)((XMaximum - XMinimum) / XGridStep);
            for (int v = 1; v <= vCount; v++)
            {
                var x = v * XGridStep * xTransform;
                plotGrid.Children.Add(new Line() { X1 = x, Y1 = 0, X2 = x, Y2 = height, Stroke = gridStroke });
            }
            //Draw horisontal grid
            int hCount = (int)((YMaximum - YMinimum) / YGridStep);
            for (int h = 0; h < hCount; h++)
            {
                var y = h * YGridStep * yTransform;
                plotGrid.Children.Add(new Line() { X1 = 0, Y1 = y, X2 = width, Y2 = y, Stroke = gridStroke });
            }
            //Horisontal Axis arrow
            plotGrid.Children.Add(new Line() { X1 = 0, Y1 = height, X2 = width, Y2 = height, Stroke = axisStroke });
            plotGrid.Children.Add(new Line() { X1 = width - 9, Y1 = height - 3, X2 = width, Y2 = height, Stroke = axisStroke });
            plotGrid.Children.Add(new Line() { X1 = width - 9, Y1 = height + 3, X2 = width, Y2 = height, Stroke = axisStroke });
            //Vertical Axis arrow
            plotGrid.Children.Add(new Line() { X1 = 0, Y1 = height, X2 = 0, Y2 = 0, Stroke = axisStroke });
            plotGrid.Children.Add(new Line() { X1 = -3, Y1 = 9, X2 = 0, Y2 = 0, Stroke = axisStroke });
            plotGrid.Children.Add(new Line() { X1 = +3, Y1 = 9, X2 = 0, Y2 = 0, Stroke = axisStroke });
        }
        void plotGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawGrid(e.NewSize.Width, e.NewSize.Height);
            DrawLimitLine(e.NewSize.Width, e.NewSize.Height);
        }

        private void plotTarget_MouseMove(object sender, MouseEventArgs e)
        {
            _mouse_X = e.GetPosition(plotTarget).X;
            _mouse_Y = e.GetPosition(plotTarget).Y;
            DrawMouseCross();
        }

        private void DrawMouseCross()
        {
            var axisStroke = Brushes.Gray;

            //Draw vertical line
            if (_vLine == null)
            {
                _vLine = new Line() { X1 = _mouse_X, Y1 = 0, X2 = _mouse_X, Y2 = plotTarget.ActualHeight, Stroke = axisStroke };
                plotTarget.Children.Add(_vLine);
            }
            else
            {
                _vLine.X1 = _mouse_X;
                _vLine.X2 = _mouse_X;
                _vLine.Y1 = 0;
                _vLine.Y2 = plotTarget.ActualHeight;
            }

            //Draw horisontal line
            if (_hLine == null)
            {
                _hLine = new Line() { X1 = _mouse_X, Y1 = 0, X2 = _mouse_X, Y2 = plotTarget.ActualHeight, Stroke = axisStroke };
                plotTarget.Children.Add(_hLine);
            }
            else
            {
                _hLine.X1 = 0;
                _hLine.X2 = plotTarget.ActualWidth;
                _hLine.Y1 = _mouse_Y;
                _hLine.Y2 = _mouse_Y;
            }

            var xVal = (XMaximum - XMinimum) / plot.ActualWidth * _mouse_X + XMinimum;

            for (int i = 0; i < ValueMembers.Count; i++)
            {
                double? valueY = GetValueY(ValueMembers[i], xVal);
                if (!_textBoxesForValueMembers.ContainsKey(ValueMembers[i]))
                {
                    var textBlock = new TextBlock()
                        {
                            Margin = new Thickness(2, 2, 2, 2),
                            Foreground = ValueMembers[i].Color,
                            FontWeight = FontWeights.Bold
                        };
                    _textBoxesForValueMembers.Add(ValueMembers[i], textBlock);
                    values.Children.Add(textBlock);
                }

                _textBoxesForValueMembers[ValueMembers[i]].Text = valueY == null ? "" : valueY.Value.ToString("F");
            }

            if (_XValueTextBox == null)
            {
                _XValueTextBox = new TextBlock()
                {
                    Margin = new Thickness(2, 2, 2, 2),
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };
                values.Children.Add(_XValueTextBox);
            }
            _XValueTextBox.Text = xVal.ToString("F");
        }
        double? GetValueY(ValueMemberDefinition vm, double x)
        {
            if (rawData == null)
                return null;
            var leftVal = rawData.Where(v => v.ValueMember == vm & v.Data >= x).OrderBy(v => v.Data).FirstOrDefault();
            var rightVal = rawData.Where(v => v.ValueMember == vm & v.Data <= x).OrderByDescending(v => v.Data).FirstOrDefault();
            if (leftVal == null | rightVal == null)
                return null;
            return (leftVal.Value - rightVal.Value) / (leftVal.Data - rightVal.Data) * (x - leftVal.Data) + leftVal.Value;
        }
    }
}
