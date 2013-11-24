﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using FlagMaker.Divisions;
using FlagMaker.Overlays;
using FlagMaker.Overlays.OverlayTypes.ShapeTypes;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace FlagMaker
{
	public partial class MainWindow
	{
		private int _ratioHeight, _ratioWidth;

		private Division _division;
		private ObservableCollection<ColorItem> _standardColors;
		private ObservableCollection<ColorItem> _availableColors;

		private bool _isLoading;
		private bool _showGrid;

		private Flag Flag
		{
			get
			{
				return new Flag("flag", new Ratio(_ratioWidth, _ratioHeight), (Ratio)cmbGridSize.SelectedItem, _division,
					lstOverlays.Children.OfType<OverlayControl>().Select(c => c.Overlay));
			}
		}

		private readonly string _headerText;
		private string _filename;
		private bool _isUnsaved;

		public static readonly RoutedCommand NewCommand = new RoutedCommand();
		public static readonly RoutedCommand SaveCommand = new RoutedCommand();
		public static readonly RoutedCommand SaveAsCommand = new RoutedCommand();
		public static readonly RoutedCommand OpenCommand = new RoutedCommand();

		public MainWindow()
		{
			InitializeComponent();

			var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			_headerText = string.Format(" - FlagMaker {0}.{1}", version.Major, version.Minor);
			SetTitle();

			_showGrid = false;

			SetColorsAndSliders();
			LoadPresets();
		}

		private void SetTitle()
		{
			Title = string.Format("{0}{1}{2}",
				string.IsNullOrWhiteSpace(_filename)
					? "Untitled"
					: Path.GetFileNameWithoutExtension(_filename),
					_isUnsaved ? "*" : string.Empty,
					_headerText);
		}

		#region Division

		private void DivisionColorChanged()
		{
			if (_isLoading) return;

			_division.SetColors(new List<Color>
			                    {
				                    divisionPicker1.SelectedColor,
				                    divisionPicker2.SelectedColor,
				                    divisionPicker3.SelectedColor
			                    });
			Draw();
			SetAsUnsaved();
		}

		private void DivisionSliderChanged()
		{
			if (_isLoading) return;

			divisionSliderLabel1.Text = divisionSlider1.Value.ToString();
			divisionSliderLabel2.Text = divisionSlider2.Value.ToString();
			divisionSliderLabel3.Text = divisionSlider3.Value.ToString();

			_division.SetValues(new List<double>
			                    {
				                    divisionSlider1.Value,
				                    divisionSlider2.Value,
				                    divisionSlider3.Value
			                    });
			Draw();
			SetAsUnsaved();
		}

		private void SetDivisionVisibility()
		{
			divisionPicker2.Visibility = Visibility.Collapsed;
			divisionPicker3.Visibility = Visibility.Collapsed;
			divisionPicker1.SelectedColor = _division.Colors[0];

			if (_division.Colors.Count > 1)
			{
				divisionPicker2.SelectedColor = _division.Colors[1];
				divisionPicker2.Visibility = Visibility.Visible;
				if (_division.Colors.Count > 2)
				{
					divisionPicker3.SelectedColor = _division.Colors[2];
					divisionPicker3.Visibility = Visibility.Visible;
				}
			}

			divisionSlider1.Visibility = Visibility.Collapsed;
			divisionSlider2.Visibility = Visibility.Collapsed;
			divisionSlider3.Visibility = Visibility.Collapsed;
			divisionSliderLabel1.Visibility = Visibility.Collapsed;
			divisionSliderLabel2.Visibility = Visibility.Collapsed;
			divisionSliderLabel3.Visibility = Visibility.Collapsed;

			if (_division.Values.Count > 0)
			{
				divisionSlider1.Value = _division.Values[0];
				divisionSlider1.Visibility = Visibility.Visible;
				divisionSliderLabel1.Text = _division.Values[0].ToString("#");
				divisionSliderLabel1.Visibility = Visibility.Visible;

				if (_division.Values.Count > 1)
				{
					divisionSlider2.Value = _division.Values[1];
					divisionSlider2.Visibility = Visibility.Visible;
					divisionSliderLabel2.Text = _division.Values[1].ToString("#");
					divisionSliderLabel2.Visibility = Visibility.Visible;

					if (_division.Values.Count > 2)
					{
						divisionSlider3.Value = _division.Values[2];
						divisionSlider3.Visibility = Visibility.Visible;
						divisionSliderLabel3.Text = _division.Values[2].ToString("#");
						divisionSliderLabel3.Visibility = Visibility.Visible;
					}
				}
			}
		}

		private void DivisionGridClick(object sender, RoutedEventArgs e)
		{
			_division = new DivisionGrid(divisionPicker1.SelectedColor, divisionPicker2.SelectedColor, (int)divisionSlider1.Value, (int)divisionSlider2.Value);
			SetDivisionVisibility();
			Draw();
			SetAsUnsaved();
		}

		private void DivisionFessesClick(object sender, RoutedEventArgs e)
		{
			_division = new DivisionFesses(divisionPicker1.SelectedColor, divisionPicker2.SelectedColor, divisionPicker3.SelectedColor, (int)divisionSlider1.Value, (int)divisionSlider2.Value, (int)divisionSlider3.Value);
			SetDivisionVisibility();
			Draw();
			SetAsUnsaved();
		}

		private void DivisionPalesClick(object sender, RoutedEventArgs e)
		{
			_division = new DivisionPales(divisionPicker1.SelectedColor, divisionPicker2.SelectedColor, divisionPicker3.SelectedColor, (int)divisionSlider1.Value, (int)divisionSlider2.Value, (int)divisionSlider3.Value);
			SetDivisionVisibility();
			Draw();
			SetAsUnsaved();
		}

		private void DivisionBendsForwardClick(object sender, RoutedEventArgs e)
		{
			_division = new DivisionBendsForward(divisionPicker1.SelectedColor, divisionPicker2.SelectedColor);
			SetDivisionVisibility();
			Draw();
			SetAsUnsaved();
		}

		private void DivisionBendsBackwardClick(object sender, RoutedEventArgs e)
		{
			_division = new DivisionBendsBackward(divisionPicker1.SelectedColor, divisionPicker2.SelectedColor);
			SetDivisionVisibility();
			Draw();
			SetAsUnsaved();
		}

		private void DivisionXClick(object sender, RoutedEventArgs e)
		{
			_division = new DivisionX(divisionPicker1.SelectedColor, divisionPicker2.SelectedColor);
			SetDivisionVisibility();
			Draw();
			SetAsUnsaved();
		}

		#endregion

		#region Overlays

		private void OverlayAdd(object sender, RoutedEventArgs e)
		{
			OverlayAdd(lstOverlays.Children.Count, null, false);
		}

		private void SetOverlayMargins()
		{
			for (int i = 0; i < lstOverlays.Children.Count - 1; i++)
			{
				((OverlayControl)lstOverlays.Children[i]).Margin = new Thickness(0, 0, 0, 20);
			}
		}

		private void Draw(object sender, EventArgs e)
		{
			Draw();
			SetAsUnsaved();
		}

		private void Remove(object sender, EventArgs e)
		{
			var controlToRemove = (OverlayControl)sender;
			lstOverlays.Children.Remove(controlToRemove);
			Draw();
			SetAsUnsaved();
		}

		private void MoveUp(object sender, EventArgs e)
		{
			var controlToMove = (OverlayControl)sender;
			int index = lstOverlays.Children.IndexOf(controlToMove);
			if (index == 0) return;

			var controls = new List<OverlayControl>();
			for (int i = 0; i < lstOverlays.Children.Count; i++)
			{
				if (i + 1 == index)
				{
					controls.Add((OverlayControl)lstOverlays.Children[i + 1]);
					controls.Add((OverlayControl)lstOverlays.Children[i]);
					i++;
				}
				else
				{
					controls.Add((OverlayControl)lstOverlays.Children[i]);
				}
			}

			lstOverlays.Children.Clear();
			foreach (var overlayControl in controls)
			{
				lstOverlays.Children.Add(overlayControl);
			}

			SetOverlayMargins();
			Draw();
			SetAsUnsaved();
		}

		private void MoveDown(object sender, EventArgs e)
		{
			var controlToMove = (OverlayControl)sender;
			int index = lstOverlays.Children.IndexOf(controlToMove);
			if (index == lstOverlays.Children.Count - 1) return;

			var controls = new List<OverlayControl>();
			for (int i = 0; i < lstOverlays.Children.Count; i++)
			{
				if (i == index)
				{
					controls.Add((OverlayControl)lstOverlays.Children[i + 1]);
					controls.Add((OverlayControl)lstOverlays.Children[i]);
					i++;
				}
				else
				{
					controls.Add((OverlayControl)lstOverlays.Children[i]);
				}
			}

			lstOverlays.Children.Clear();
			foreach (var overlayControl in controls)
			{
				lstOverlays.Children.Add(overlayControl);
			}

			SetOverlayMargins();
			Draw();
			SetAsUnsaved();
		}

		private void Clone(object sender, EventArgs e)
		{
			var controlToClone = (OverlayControl)sender;
			int index = lstOverlays.Children.IndexOf(controlToClone);
			OverlayAdd(index, controlToClone.Overlay, false);
		}

		private void OverlayAdd(int index, Overlay overlay, bool isLoading)
		{
			var gridSize = ((Ratio)cmbGridSize.SelectedItem);
			var newOverlay = new OverlayControl(_standardColors, _availableColors, gridSize.Width, gridSize.Height)
							 {
								 IsLoading = isLoading
							 };
			if (overlay != null)
			{
				newOverlay.SetType(overlay.Name);

				if (overlay is OverlayFlag)
				{
					newOverlay.Overlay = overlay;
				}

				newOverlay.Color = overlay.Color;
				for (int i = 0; i < overlay.Attributes.Count; i++)
				{
					newOverlay.SetSlider(i, overlay.Attributes[i].Value);
				}
			}

			newOverlay.OnDraw += Draw;
			newOverlay.OnRemove += Remove;
			newOverlay.OnMoveUp += MoveUp;
			newOverlay.OnMoveDown += MoveDown;
			newOverlay.OnClone += Clone;

			lstOverlays.Children.Insert(index, newOverlay);

			SetOverlayMargins();

			if (!_isLoading)
			{
				Draw();
				SetAsUnsaved();
			}
		}

		#endregion
		
		private void SetColorsAndSliders()
		{
			_standardColors = ColorFactory.Colors(Palette.FlagsOfAllNations, false);
			_availableColors = ColorFactory.Colors(Palette.FlagsOfTheWorld, true);

			divisionPicker1.AvailableColors = _availableColors;
			divisionPicker1.StandardColors = _standardColors;
			divisionPicker1.SelectedColor = divisionPicker1.StandardColors[1].Color;

			divisionPicker2.AvailableColors = _availableColors;
			divisionPicker2.StandardColors = _standardColors;
			divisionPicker2.SelectedColor = divisionPicker2.StandardColors[5].Color;

			divisionPicker3.AvailableColors = _availableColors;
			divisionPicker3.StandardColors = _standardColors;
			divisionPicker3.SelectedColor = divisionPicker3.StandardColors[8].Color;

			divisionPicker1.SelectedColorChanged += (sender, args) => DivisionColorChanged();
			divisionPicker2.SelectedColorChanged += (sender, args) => DivisionColorChanged();
			divisionPicker3.SelectedColorChanged += (sender, args) => DivisionColorChanged();
			divisionSlider1.ValueChanged += (sender, args) => DivisionSliderChanged();
			divisionSlider2.ValueChanged += (sender, args) => DivisionSliderChanged();
			divisionSlider3.ValueChanged += (sender, args) => DivisionSliderChanged();

			New();
		}

		private void SetRatio(int width, int height)
		{
			txtRatioHeight.Text = height.ToString();
			txtRatioWidth.Text = width.ToString();
			_ratioHeight = height;
			_ratioWidth = width;

			FillGridCombobox();
		}

		private void SetAsUnsaved()
		{
			_isUnsaved = true;
			SetTitle();
		}

		private void Draw()
		{
			canvas.Width = _ratioWidth * 200;
			canvas.Height = _ratioHeight * 200;
			Flag.Draw(canvas);
			DrawGrid();
		}

		private void DrawGrid()
		{
			canvasGrid.Children.Clear();

			if (!_showGrid) return;

			if (cmbGridSize.Items.Count == 0) return;

			var gridSize = ((Ratio)cmbGridSize.SelectedItem);

			var intervalX = canvas.Width / gridSize.Width;
			for (int x = 0; x <= gridSize.Width; x++)
			{
				var line = new Line
				{
					StrokeThickness = 3,
					X1 = 0,
					X2 = 0,
					Y1 = 0,
					Y2 = canvas.Height,
					SnapsToDevicePixels = false,
					Stroke = new SolidColorBrush(Colors.Silver)
				};
				canvasGrid.Children.Add(line);
				Canvas.SetTop(line, 0);
				Canvas.SetLeft(line, x * intervalX);
			}

			var intervalY = canvas.Height / gridSize.Height;
			for (int y = 0; y <= gridSize.Height; y++)
			{
				var line = new Line
				{
					StrokeThickness = 3,
					X1 = 0,
					X2 = canvas.Width,
					Y1 = 0,
					Y2 = 0,
					SnapsToDevicePixels = false,
					Stroke = new SolidColorBrush(Colors.Silver)
				};
				canvasGrid.Children.Add(line);
				Canvas.SetTop(line, y * intervalY);
				Canvas.SetLeft(line, 0);
			}
		}

		private void FillGridCombobox()
		{
			cmbGridSize.Items.Clear();
			for (int i = 1; i <= 20; i++)
			{
				cmbGridSize.Items.Add(new Ratio(_ratioWidth * i, _ratioHeight * i));
			}
			cmbGridSize.SelectedIndex = 0;
		}

		private void RatioTextboxChanged(object sender, TextChangedEventArgs e)
		{
			int newHeight;
			int newWidth;

			if (!int.TryParse(txtRatioHeight.Text, out newHeight))
			{
				_ratioHeight = 1;
			}

			if (!int.TryParse(txtRatioWidth.Text, out newWidth))
			{
				_ratioWidth = 1;
			}

			if (newHeight < 1)
			{
				_ratioHeight = 1;
				txtRatioHeight.Text = "1";
			}
			else
			{
				_ratioHeight = newHeight;
			}

			if (newWidth < 1)
			{
				_ratioWidth = 1;
				txtRatioWidth.Text = "1";
			}
			else
			{
				_ratioWidth = newWidth;
			}

			if (!_isLoading)
			{
				Draw();
				SetAsUnsaved();
			}

			FillGridCombobox();
		}

		private void GridSizeDropdownChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cmbGridSize.Items.Count == 0) return;

			var gridSize = ((Ratio)cmbGridSize.SelectedItem);
			int sliderMaxX = gridSize.Width;
			int sliderMaxY = gridSize.Height;
			int sliderMax = Math.Max(sliderMaxX, sliderMaxY);

			divisionSlider1.Maximum = sliderMax;
			divisionSlider2.Maximum = sliderMax;
			divisionSlider3.Maximum = sliderMax;

			_division.SetMaximum(sliderMaxX, sliderMaxY);

			foreach (var overlay in lstOverlays.Children)
			{
				((OverlayControl)overlay).SetMaximum(sliderMaxX, sliderMaxY);
			}

			if (!_isLoading)
			{
				Draw();
				SetAsUnsaved();
			}
		}

		#region Export

		private void MenuExportPngClick(object sender, RoutedEventArgs e)
		{
			var dialog = new ExportPng(new Ratio(_ratioWidth, _ratioHeight)) { Owner = this };
			if (!(dialog.ShowDialog() ?? false)) return;

			var dimensions = new Size(dialog.PngWidth, dialog.PngHeight);

			var dlg = new SaveFileDialog
			{
				FileName = "Untitled",
				DefaultExt = ".png",
				Filter = "PNG (*.png)|*.png"
			};

			bool? result = dlg.ShowDialog();
			if (!((bool)result)) return;

			// Create a full copy of the canvas so the
			// scaling of the existing canvas and
			// grid don't ge messed up
			string gridXaml = XamlWriter.Save(canvas);
			var stringReader = new StringReader(gridXaml);
			XmlReader xmlReader = XmlReader.Create(stringReader);
			var newGrid = (Canvas)XamlReader.Load(xmlReader);

			ExportToPng(new Uri(dlg.FileName), newGrid, dimensions);
		}

		private static void ExportToPng(Uri path, FrameworkElement surface, Size newSize)
		{
			if (path == null) return;

			// Get original size of canvas
			var size = new Size(surface.Width, surface.Height);

			// Appy scaling for desired PNG size
			surface.LayoutTransform = new ScaleTransform(newSize.Width / size.Width, newSize.Height / size.Height);

			surface.Measure(size);
			surface.Arrange(new Rect(newSize));

			var renderBitmap =
				new RenderTargetBitmap(
					(int)newSize.Width,
					(int)newSize.Height,
					96d,
					96d,
					PixelFormats.Pbgra32);
			renderBitmap.Render(surface);

			using (var outStream = new FileStream(path.LocalPath, FileMode.Create))
			{
				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
				encoder.Save(outStream);
			}
		}

		private void MenuExportSvgClick(object sender, RoutedEventArgs e)
		{
			var dlg = new SaveFileDialog
			{
				FileName = "Untitled",
				DefaultExt = ".svg",
				Filter = "SVG (*.svg)|*.svg"
			};

			bool? result = dlg.ShowDialog();
			if (!((bool)result)) return;

			ExportToSvg(new Uri(dlg.FileName));
		}

		private void ExportToSvg(Uri path)
		{
			Flag.ExportToSvg(path);
		}

		#endregion

		#region Load / save

		private void MenuNewClick(object sender, RoutedEventArgs e)
		{
			New();
			SetTitle();
		}

		private void New()
		{
			if (CheckUnsaved()) return;
			PlainPreset(2, 2);
			divisionPicker1.SelectedColor = divisionPicker1.StandardColors[1].Color;
			divisionPicker2.SelectedColor = divisionPicker2.StandardColors[5].Color;
			lstOverlays.Children.Clear();
			SetRatio(3, 2);
			txtName.Text = "Untitled";
			_filename = string.Empty;

			_isUnsaved = false;
			SetTitle();
		}

		private void MenuSaveClick(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_filename))
			{
				MenuSaveAsClick(sender, e);
			}
			else
			{
				Save();
			}

			SetTitle();
		}

		private void MenuSaveAsClick(object sender, RoutedEventArgs e)
		{
			var dlg = new SaveFileDialog
						  {
							  FileName = string.IsNullOrWhiteSpace(_filename) ? "Untitled" : Path.GetFileNameWithoutExtension(_filename),
							  DefaultExt = ".flag",
							  Filter = "Flag (*.flag)|*.flag|All files (*.*)|*.*"
						  };

			bool? result = dlg.ShowDialog();
			if (!((bool)result)) return;

			_filename = dlg.FileName;
			SetTitle();
			Save();
		}

		private void Save()
		{
			using (var sr = new StreamWriter(_filename, false, Encoding.Unicode))
			{
				sr.WriteLine("name={0}", string.IsNullOrWhiteSpace(txtName.Text) ? Path.GetFileNameWithoutExtension(_filename) : txtName.Text);
				sr.WriteLine("ratio={0}:{1}", txtRatioHeight.Text, txtRatioWidth.Text);
				sr.WriteLine("gridSize={0}", cmbGridSize.SelectedItem);

				sr.WriteLine();

				sr.WriteLine("division");
				sr.WriteLine("type={0}", _division.Name);
				sr.WriteLine("color1={0}", divisionPicker1.SelectedColor.ToHexString());
				sr.WriteLine("color2={0}", divisionPicker2.SelectedColor.ToHexString());
				sr.WriteLine("color3={0}", divisionPicker3.SelectedColor.ToHexString());
				sr.WriteLine("size1={0}", divisionSlider1.Value);
				sr.WriteLine("size2={0}", divisionSlider2.Value);
				sr.WriteLine("size3={0}", divisionSlider3.Value);

				foreach (var overlay in from object child in lstOverlays.Children select ((OverlayControl)child))
				{
					sr.WriteLine();
					sr.WriteLine("overlay");
					sr.WriteLine("type={0}", overlay.Overlay.Name);
					if (overlay.Overlay.Name == "flag") sr.WriteLine("path={0}", ((OverlayFlag)overlay.Overlay).Path);
					else sr.WriteLine("color={0}", overlay.Color.ToHexString());

					for (int i = 0; i < overlay.Overlay.Attributes.Count(); i++)
					{
						sr.WriteLine("size{0}={1}", i + 1, overlay.Overlay.Attributes[i].Value);
					}
				}
			}

			_isUnsaved = false;
			LoadPresets();
		}

		private void MenuOpenClick(object sender, RoutedEventArgs e)
		{
			if (CheckUnsaved()) return;
			var path = Flag.GetFlagPath();
			if (!string.IsNullOrWhiteSpace(path))
			{
				LoadFlagFromFile(path, true);
			}
			SetTitle();
		}

		// Cancel if returns true
		private bool CheckUnsaved()
		{
			if (!_isUnsaved) return false;

			string message = string.Format("Save changes to \"{0}\"?",
				string.IsNullOrWhiteSpace(_filename)
					? "untitled"
					: Path.GetFileNameWithoutExtension(_filename));

			var result = MessageBox.Show(message, "FlagMaker", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
			if (result == MessageBoxResult.Yes)
			{
				MenuSaveClick(null, null);
			}

			return result == MessageBoxResult.Cancel;
		}

		private void LoadFlagFromFile(string filename, bool isLoading)
		{
			var flag = Flag.LoadFromFile(filename);
			_isLoading = true;

			txtRatioHeight.Text = flag.Ratio.Height.ToString(CultureInfo.InvariantCulture);
			txtRatioWidth.Text = flag.Ratio.Width.ToString(CultureInfo.InvariantCulture);
			for (int i = 0; i < cmbGridSize.Items.Count; i++)
			{
				if (((Ratio)cmbGridSize.Items[i]).Width != flag.GridSize.Width) continue;
				cmbGridSize.SelectedIndex = i;
				break;
			}

			_division = flag.Division;
			SetDivisionVisibility();

			lstOverlays.Children.Clear();
			foreach (var overlay in flag.Overlays)
			{
				OverlayAdd(lstOverlays.Children.Count, overlay, isLoading);
			}

			txtName.Text = flag.Name;
			_filename = filename;
			_isUnsaved = false;

			Draw();
			_isLoading = false;
			foreach (var control in lstOverlays.Children.OfType<OverlayControl>())
			{
				control.IsLoading = false;
			}
		}

		#endregion

		#region Presets

		private void PresetChanged(object sender, SelectionChangedEventArgs e)
		{
			cmbPresets.SelectedIndex = -1;
		}

		private void PresetBlank(object sender, RoutedEventArgs e)
		{
			PlainPreset(1, 1);
		}

		private void PresetHorizontal(object sender, RoutedEventArgs e)
		{
			PlainPreset(1, 2);
		}

		private void PresetVertical(object sender, RoutedEventArgs e)
		{
			PlainPreset(2, 1);
		}

		private void PresetQuad(object sender, RoutedEventArgs e)
		{
			PlainPreset(2, 2);
		}

		private void PresetStripes(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < cmbGridSize.Items.Count; i++)
			{
				if (((Ratio)cmbGridSize.Items[i]).Width >= 7)
				{
					cmbGridSize.SelectedIndex = i;
					break;
				}
			}

			PlainPreset(1, 7);
		}

		private void PlainPreset(int slider1, int slider2)
		{
			DivisionGridClick(null, null);
			divisionSlider1.Value = slider1;
			divisionSlider2.Value = slider2;
			divisionSlider3.Value = 1;
		}

		private void LoadPresets()
		{
			try
			{
				var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "presets").Where(f => f.EndsWith(".flag"));

				var presets = new Dictionary<string, string>();
				foreach (var file in files)
				{
					var name = GetPresetFlagName(file);
					if (!string.IsNullOrWhiteSpace(name))
					{
						presets.Add(file, name);
					}
				}

				mnuWorldFlagPresets.Items.Clear();
				foreach (var menuItem in presets.OrderBy(p => p.Value).Select(preset => new MenuItem { Header = preset.Value, ToolTip = preset.Key }))
				{
					menuItem.Click += LoadPreset;
					mnuWorldFlagPresets.Items.Add(menuItem);
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Couldn't load presets. Check for a Presets folder in the application directory.");
			}
		}

		private void LoadPreset(object sender, RoutedEventArgs routedEventArgs)
		{
			if (CheckUnsaved()) return;
			var menuItem = (MenuItem)sender;
			LoadFlagFromFile(menuItem.ToolTip.ToString(), true);
			SetTitle();
		}

		private static string GetPresetFlagName(string filename)
		{
			using (var sr = new StreamReader(filename))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if (line.StartsWith("name="))
					{
						return line.Split('=')[1];
					}
				}
			}

			return string.Empty;
		}

		#endregion

		private void MainWindowOnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			viewbox.MaxHeight = Height - 100;
		}

		private void GridOnChanged(object sender, RoutedEventArgs e)
		{
			_showGrid = !_showGrid;

			if (_showGrid)
			{
				btnGrid.Background = new SolidColorBrush(Colors.LightSkyBlue);
			}
			else
			{
				btnGrid.ClearValue(BackgroundProperty);
			}

			DrawGrid();
		}
	}
}
