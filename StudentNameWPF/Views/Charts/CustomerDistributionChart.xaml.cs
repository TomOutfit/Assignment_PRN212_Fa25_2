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
    public partial class CustomerDistributionChart : UserControl
    {
        private List<CustomerDistributionData> _data = new();
        
        public SeriesCollection SeriesCollection { get; set; }
        
        public CustomerDistributionChart()
        {
            InitializeComponent();
            DataContext = this;
            SeriesCollection = new SeriesCollection();
        }
        
        public void UpdateData(List<CustomerDistributionData> data)
        {
            _data = data ?? new List<CustomerDistributionData>();
            System.Diagnostics.Debug.WriteLine($"CustomerDistributionChart: Updating with {_data.Count} data points");
            UpdateChart();
        }
        
        private void UpdateChart()
        {
            if (!_data.Any())
            {
                NoDataText.Visibility = Visibility.Visible;
                Chart.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine("CustomerDistributionChart: No data to display");
                return;
            }
            
            NoDataText.Visibility = Visibility.Collapsed;
            Chart.Visibility = Visibility.Visible;
            System.Diagnostics.Debug.WriteLine($"CustomerDistributionChart: Drawing chart with {_data.Count} data points");
            
            // Limit data to prevent overcrowding
            var displayData = _data.Take(8).ToList(); // Show max 8 customers to prevent overlap
            
            SeriesCollection.Clear();
            
            var colors = new[]
            {
                System.Windows.Media.Brushes.Red,
                System.Windows.Media.Brushes.Blue,
                System.Windows.Media.Brushes.Green,
                System.Windows.Media.Brushes.Orange,
                System.Windows.Media.Brushes.Purple,
                System.Windows.Media.Brushes.Teal,
                System.Windows.Media.Brushes.Pink,
                System.Windows.Media.Brushes.Brown
            };
            
            foreach (var (item, index) in displayData.Select((item, index) => (item, index)))
            {
                SeriesCollection.Add(new PieSeries
                {
                    Title = item.CustomerName,
                    Values = new ChartValues<double> { item.BookingCount },
                    Fill = colors[index % colors.Length],
                    DataLabels = false, // Disable data labels to prevent overlap
                    LabelPoint = point => $"{item.CustomerName}: {point.Y} bookings"
                });
            }
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateChart();
        }
        
        public void ForceRedraw()
        {
            System.Diagnostics.Debug.WriteLine("CustomerDistributionChart: Force redraw requested");
            UpdateChart();
        }
    }
}
