using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlagMaker.Overlays.OverlayTypes.ShapeTypes
{
	internal class OverlayEllipse : OverlayShape
	{
		public OverlayEllipse(int maximumX, int maximumY)
			: base(maximumX, maximumY)
		{
		}

		public OverlayEllipse(Color color, double x, double y, double width, double height, int maximumX, int maximumY)
			: base(color, x, y, width, height, maximumX, maximumY)
		{
		}

		public override string Name { get { return "ellipse"; } }

		public override void Draw(Canvas canvas)
		{
			var width = canvas.Width * (Attributes.Get("Width").Value / MaximumX);
			var height = Attributes.Get("Height").Value == 0
							 ? width
							 : canvas.Height * (Attributes.Get("Height").Value / MaximumY);

			var path = new Ellipse
			{
				Fill = new SolidColorBrush(Color),
				Width = width,
				Height = height,
				SnapsToDevicePixels = true
			};
			canvas.Children.Add(path);

			Canvas.SetLeft(path, (canvas.Width * (Attributes.Get("X").Value / MaximumX)) - width / 2);
			Canvas.SetTop(path, (canvas.Height * (Attributes.Get("Y").Value / MaximumY)) - height / 2);
		}

		public override string ExportSvg(int width, int height)
		{
			var w = width * (Attributes.Get("Width").Value / MaximumX);
			var h = Attributes.Get("Height").Value == 0
							 ? w
							 : height * (Attributes.Get("Height").Value / MaximumY);

			double x = width * (Attributes.Get("X").Value / MaximumX);
			double y = height * (Attributes.Get("Y").Value / MaximumY);

			return string.Format("<ellipse cx=\"{0}\" cy=\"{1}\" rx=\"{2}\" ry=\"{3}\" fill=\"#{4}\" />",
				x, y, w / 2, h / 2,
				Color.ToHexString());
		}

		public override IEnumerable<Shape> Thumbnail
		{
			get
			{
				return new List<Shape>
				       {
					       new Ellipse
					       {
						       Width = 20,
						       Height = 20
					       }
				       };
			}
		}
	}
}