using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlagMaker.Overlays.OverlayTypes
{
	internal class OverlayCross : Overlay
	{
		public OverlayCross(int maximumX, int maximumY)
			: base(new List<Attribute>
			       {
				       new Attribute("X", true, 1, true),
				       new Attribute("Y", true, 1, false),
				       new Attribute("Thickness", true, 1, true)
			       }, maximumX, maximumY)
		{
		}

		public OverlayCross(Color color, int thickness, int x, int y, int maximumX, int maximumY)
			: base(color, new List<Attribute>
			             {
				             new Attribute("X", true, x, true),
				             new Attribute("Y", true, y, false),
				             new Attribute("Thickness", true, thickness, true)
			             }, maximumX, maximumY)
		{
		}

		public override string Name { get { return "cross"; } }

		public override void Draw(Canvas canvas)
		{
			double thick = canvas.Width * ((Attributes.Get("Thickness").Value + 1) / (MaximumX * 2));

			var vertical = new Rectangle
							   {
								   Fill = new SolidColorBrush(Color),
								   Width = thick,
								   Height = canvas.Height,
								   SnapsToDevicePixels = true
							   };
			canvas.Children.Add(vertical);

			var horizontal = new Rectangle
								 {
									 Fill = new SolidColorBrush(Color),
									 Width = canvas.Width,
									 Height = thick,
									 SnapsToDevicePixels = true
								 };
			canvas.Children.Add(horizontal);

			Canvas.SetLeft(vertical, canvas.Width * (Attributes.Get("X").Value / MaximumX) - thick / 2);
			Canvas.SetTop(horizontal, canvas.Height * (Attributes.Get("Y").Value / MaximumY) - thick / 2);
		}

		public override void SetValues(List<double> values)
		{
			Attributes.Get("X").Value = values[0];
			Attributes.Get("Y").Value = values[1];
			Attributes.Get("Thickness").Value = values[2];
		}

		public override string ExportSvg(int width, int height)
		{
			double thick = width * ((Attributes.Get("Thickness").Value + 1) / (MaximumX * 2));

			double x = width * (Attributes.Get("X").Value / MaximumX) - thick / 2;
			double y = height * (Attributes.Get("Y").Value / MaximumY) - thick / 2;

			return string.Format("<rect width=\"{0}\" height=\"{1}\" x=\"{2}\" y=\"0\" fill=\"#{5}\" /><rect width=\"{3}\" height=\"{0}\" x=\"0\" y=\"{4}\" fill=\"#{5}\" />",
				thick, height, x, width, y, Color.ToHexString());
		}

		public override IEnumerable<Shape> Thumbnail
		{
			get
			{
				return new Shape[]
				       {
						   new Line
						   {
							   StrokeThickness = 5,
							   X1 = 10,
							   X2 = 10,
							   Y1 = 0,
							   Y2 = 20
						   },
						   new Line
						   {
							   StrokeThickness = 5,
							   X1 = 0,
							   X2 = 30,
							   Y1 = 10,
							   Y2 = 10
						   }
				       };
			}
		}
	}
}