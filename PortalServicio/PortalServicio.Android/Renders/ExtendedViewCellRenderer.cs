using Android.Content;
using Android.Views;
using Xamarin.Forms;
using PortalServicio;
using PortalServicio.Droid.Renders;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(ExtendedViewCell), typeof(ExtendedViewCellRenderer))]
namespace PortalServicio.Droid.Renders
{
    public class ExtendedViewCellRenderer : ViewCellRenderer
    {

        private Android.Views.View _cellCore;
        private Drawable _unselectedBackground;
        private bool _selected;

        protected override Android.Views.View GetCellCore(Cell item,
                                                          Android.Views.View convertView,
                                                          ViewGroup parent,
                                                          Context context)
        {
            _cellCore = base.GetCellCore(item, convertView, parent, context);
            _selected = false;
            _unselectedBackground = _cellCore.Background;
            return _cellCore;
        }

        protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnCellPropertyChanged(sender, args);
            if (args.PropertyName == "IsSelected")
            {
                _selected = !_selected;
                if (_selected)
                {
                    var extendedViewCell = sender as ExtendedViewCell;
                    _cellCore.SetBackgroundColor(extendedViewCell.SelectedBackgroundColor.ToAndroid());
                }
                else
                    _cellCore.SetBackground(_unselectedBackground);
            }
        }
    }

}

/* For IOS
[assembly: ExportRenderer(typeof(ExtendedViewCell), typeof(ExtendedViewCellRenderer))]
namespace xamformsdemo.iOS.CustomRenderers
{
  public class ExtendedViewCellRenderer : ViewCellRenderer
  {
    public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
    {
      var cell = base.GetCell(item, reusableCell, tv);
      var view = item as ExtendedViewCell;
      cell.SelectedBackgroundView = new UIView
      {
        BackgroundColor = view.SelectedBackgroundColor.ToUIColor(),
      };

      return cell;
    }

  }
}

    */
