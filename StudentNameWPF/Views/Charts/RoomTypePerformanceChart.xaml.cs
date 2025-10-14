using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StudentNameWPF.Views.Charts
{
    public partial class RoomTypePerformanceChart : UserControl
    {
        private List<RoomTypePerformanceData> _data = new();
        
        public RoomTypePerformanceChart()
        {
            InitializeComponent();
        }
        
        public void UpdateData(List<RoomTypePerformanceData> data)
        {
            _data = data ?? new List<RoomTypePerformanceData>();
            DrawChart();
        }
        
        private void DrawChart()
        {
            ChartCanvas.Children.Clear();
            
            if (!_data.Any())
            {
                NoDataText.Visibility = Visibility.Visible;
                return;
            }
            
            NoDataText.Visibility = Visibility.Collapsed;
            
            var canvasWidth = ChartCanvas.ActualWidth > 0 ? ChartCanvas.ActualWidth : 400;
            var canvasHeight = ChartCanvas.ActualHeight > 0 ? ChartCanvas.ActualHeight : 300;
            
            if (canvasWidth <= 0 || canvasHeight <= 0) return;
            
            var margin = 40;
            var chartWidth = canvasWidth - 2 * margin;
            var chartHeight = canvasHeight - 2 * margin;
            
            var maxRoomCount = _data.Max(d => d.RoomCount);
            var maxBookingCount = _data.Max(d => d.BookingCount);
            var maxValue = Math.Max(maxRoomCount, maxBookingCount);
            if (maxValue <= 0) maxValue = 1;
            
            // Draw axes
            var xAxis = new Line
            {
                X1 = margin,
                Y1 = margin + chartHeight,
                X2 = margin + chartWidth,
                Y2 = margin + chartHeight,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            ChartCanvas.Children.Add(xAxis);
            
            var yAxis = new Line
            {
                X1 = margin,
                Y1 = margin,
                X2 = margin,
                Y2 = margin + chartHeight,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            ChartCanvas.Children.Add(yAxis);
            
            // Draw bars
            var barWidth = chartWidth / _data.Count * 0.8;
            var barSpacing = chartWidth / _data.Count;
            
            for (int i = 0; i < _data.Count; i++)
            {
                var item = _data[i];
                var x = margin + (i * barSpacing) + (barSpacing - barWidth) / 2;
                
                // Room count bar
                var roomHeight = (double)item.RoomCount / maxValue * chartHeight;
                var roomY = margin + chartHeight - roomHeight;
                
                var roomBar = new Rectangle
                {
                    Width = barWidth / 2 - 2,
                    Height = roomHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(54, 162, 235)),
                    Stroke = new SolidColorBrush(Color.FromRgb(54, 162, 235)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(roomBar, x);
                Canvas.SetTop(roomBar, roomY);
                ChartCanvas.Children.Add(roomBar);
                
                // Booking count bar
                var bookingHeight = (double)item.BookingCount / maxValue * chartHeight;
                var bookingY = margin + chartHeight - bookingHeight;
                
                var bookingBar = new Rectangle
                {
                    Width = barWidth / 2 - 2,
                    Height = bookingHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(255, 99, 132)),
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 99, 132)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(bookingBar, x + barWidth / 2 + 2);
                Canvas.SetTop(bookingBar, bookingY);
                ChartCanvas.Children.Add(bookingBar);
                
                // Add value labels
                var roomLabel = new TextBlock
                {
                    Text = item.RoomCount.ToString(),
                    FontSize = 8,
                    Foreground = Brushes.Blue,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(roomLabel, x);
                Canvas.SetTop(roomLabel, roomY - 15);
                ChartCanvas.Children.Add(roomLabel);
                
                var bookingLabel = new TextBlock
                {
                    Text = item.BookingCount.ToString(),
                    FontSize = 8,
                    Foreground = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(bookingLabel, x + barWidth / 2 + 2);
                Canvas.SetTop(bookingLabel, bookingY - 15);
                ChartCanvas.Children.Add(bookingLabel);
                
                // Add room type label
                var typeLabel = new TextBlock
                {
                    Text = item.RoomTypeName,
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(typeLabel, x);
                Canvas.SetTop(typeLabel, margin + chartHeight + 5);
                ChartCanvas.Children.Add(typeLabel);
            }
            
            // Add legend
            var legendY = margin + chartHeight + 30;
            var legendX = margin;
            
            // Room count legend
            var roomLegend = new Rectangle
            {
                Width = 15,
                Height = 15,
                Fill = new SolidColorBrush(Color.FromRgb(54, 162, 235))
            };
            Canvas.SetLeft(roomLegend, legendX);
            Canvas.SetTop(roomLegend, legendY);
            ChartCanvas.Children.Add(roomLegend);
            
            var roomLegendText = new TextBlock
            {
                Text = "Room Count",
                FontSize = 10,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(roomLegendText, legendX + 20);
            Canvas.SetTop(roomLegendText, legendY);
            ChartCanvas.Children.Add(roomLegendText);
            
            // Booking count legend
            var bookingLegend = new Rectangle
            {
                Width = 15,
                Height = 15,
                Fill = new SolidColorBrush(Color.FromRgb(255, 99, 132))
            };
            Canvas.SetLeft(bookingLegend, legendX + 100);
            Canvas.SetTop(bookingLegend, legendY);
            ChartCanvas.Children.Add(bookingLegend);
            
            var bookingLegendText = new TextBlock
            {
                Text = "Booking Count",
                FontSize = 10,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(bookingLegendText, legendX + 120);
            Canvas.SetTop(bookingLegendText, legendY);
            ChartCanvas.Children.Add(bookingLegendText);
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (_data.Any())
            {
                DrawChart();
            }
        }
    }
}
