using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace StudentNameWPF.Views.Charts
{
    public partial class MonthlyRevenueChart : UserControl
    {
        private List<MonthlyRevenueData> _data = new();
        
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Func<double, string> XFormatter { get; set; }
        
        public MonthlyRevenueChart()
        {
            InitializeComponent();
            DataContext = this;
            
            SeriesCollection = new SeriesCollection();
            Labels = new string[0];
            YFormatter = value => $"${value:N0}";
            XFormatter = value => value.ToString();
        }
        
        public void UpdateData(List<MonthlyRevenueData> data)
        {
            _data = data ?? new List<MonthlyRevenueData>();
            System.Diagnostics.Debug.WriteLine($"MonthlyRevenueChart: Updating with {_data.Count} data points");
            UpdateChart();
        }
        
        private void UpdateChart()
        {
            if (!_data.Any())
            {
                NoDataText.Visibility = Visibility.Visible;
                Chart.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine("MonthlyRevenueChart: No data to display");
                return;
            }
            
            NoDataText.Visibility = Visibility.Collapsed;
            Chart.Visibility = Visibility.Visible;
            System.Diagnostics.Debug.WriteLine($"MonthlyRevenueChart: Drawing chart with {_data.Count} data points");
            
            // Create labels for X-axis (limit to prevent overlap)
            var maxLabels = Math.Min(_data.Count, 12); // Show max 12 labels to prevent overlap
            var step = Math.Max(1, _data.Count / maxLabels);
            Labels = _data.Where((item, index) => index % step == 0 || index == _data.Count - 1)
                          .Select(d => d.MonthName)
                          .ToArray();
            
            // Create revenue values
            var revenueValues = _data.Select(d => (double)d.Revenue).ToArray();
            
            // Create series
            SeriesCollection.Clear();
            
            // Add column series for revenue bars
            SeriesCollection.Add(new ColumnSeries
            {
                Title = "Monthly Revenue",
                Values = new ChartValues<double>(revenueValues),
                Fill = System.Windows.Media.Brushes.LightBlue,
                Stroke = System.Windows.Media.Brushes.DodgerBlue,
                StrokeThickness = 1,
                DataLabels = false // Disable data labels to prevent overlap
            });
            
            // Add line series for trend
            SeriesCollection.Add(new LineSeries
            {
                Title = "Revenue Trend",
                Values = new ChartValues<double>(revenueValues),
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 3,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 6,
                DataLabels = false // Disable data labels to prevent overlap
            });
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateChart();
        }
        
        public void ForceRedraw()
        {
            System.Diagnostics.Debug.WriteLine("MonthlyRevenueChart: Force redraw requested");
            UpdateChart();
        }
    }
}
