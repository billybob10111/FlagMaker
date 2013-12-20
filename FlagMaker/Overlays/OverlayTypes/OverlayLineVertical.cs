using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlagMaker.Overlays.OverlayTypes
{
	internal class OverlayLineVertical : Overlay
	{
		public OverlayLineVertical(int maximumX, int maximumY)
			: base(new List<Attribute>
			       {
				       new Attribute("X", true, 1, true),
				       new Attribute("Thickness", true, 1, true)
			       }, maximumX, maximumY)
		{
		}

		public OverlayLineVertical(Color color, double thickness, double x, int maximumX, int maximumY)
			: base(color, new List<Attribute>
			             {
				             new Attribute("X", true, x, true),
				             new Attribute("Thickness", true, thickness, true)
			             }, maximumX, maximumY)
		{
		}

		public override string Name { get { return "line vertical"; } }

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

			Canvas.SetLeft(vertical, canvas.Width * (Attributes.Get("X").Value / MaximumX) - thick / 2);
		}

		public override void SetValues(List<double> values)
		{
			Attributes.Get("X").Value = values[0];
			Attributes.Get("Thickness").Value = values[1];
		}

		public override string ExportSvg(int width, int height)
		{
			double thick = height * ((Attributes.Get("Thickness").Value + 1) / (MaximumX * 2));

			double x = MaximumX % 2 == 0
				? width * (Attributes.Get("X").Value / MaximumX) - thick / 2
				: width * (Attributes.Get("X").Value / (MaximumX + 1)) - thick / 2;

			return string.Format(CultureInfo.InvariantCulture, "<rect height=\"{0}\" width=\"{1}\" x=\"{2}\" y=\"0\" fill=\"#{3}\" />",
				height, thick, x, Color.ToHexString());
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
							   X1 = 15,
							   X2 = 15,
							   Y1 = 0,
							   Y2 = 20
						   }
				       };
			}
		}
	}
}