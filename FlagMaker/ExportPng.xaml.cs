﻿using System.Windows;
using System.Windows.Controls;

namespace FlagMaker
{
	public partial class ExportPng
	{
		private readonly Ratio _ratio;
		private int _width;
		private int _height;

		public ExportPng(Ratio ratio)
		{
			InitializeComponent();

			const int multiplier = 100;
			_ratio = ratio;
			PngWidth = ratio.Width * multiplier;
			PngHeight = ratio.Height * multiplier;
		}

		public int PngWidth
		{
			get { return _width; }
			set
			{
				_width = value;
				txtWidth.Text = _width.ToString();
			}
		}

		public int PngHeight
		{
			get { return _height; }
			set
			{
				_height = value;
				txtHeight.Text = _height.ToString();
			}
		}

		private void WidthChanged(object sender, TextChangedEventArgs e)
		{
			int newWidth;

			if (int.TryParse(txtWidth.Text, out newWidth))
			{
				_width = newWidth;
				PngHeight = (int)((_ratio.Height / (double)_ratio.Width) * _width);
			}
			else
			{
				txtWidth.Text = _width.ToString();
			}
		}

		private void HeightChanged(object sender, TextChangedEventArgs e)
		{
			int newHeight;

			if (int.TryParse(txtHeight.Text, out newHeight))
			{
				_width = newHeight;
				PngWidth = (int)((_ratio.Width / (double)_ratio.Height) * _height);
			}
			else
			{
				txtHeight.Text = _height.ToString();
			}
		}

		private void OkClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}