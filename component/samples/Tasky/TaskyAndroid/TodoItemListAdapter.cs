using System.Collections.Generic;
using Android.App;
using Android.Widget;
using Tasky.Shared;

namespace TaskyAndroid.ApplicationLayer 
{
	/// <summary>
	/// Adapter that presents Tasks in a row-view
	/// </summary>
	public class TodoItemListAdapter : BaseAdapter<TodoItem> 
	{
		Activity context = null;
		IList<TodoItem> tasks = new List<TodoItem>();
		
		public TodoItemListAdapter (Activity context, IList<TodoItem> tasks) : base ()
		{
			this.context = context;
			this.tasks = tasks;
		}
		
		public override TodoItem this[int position]
		{
			get { return tasks[position]; }
		}
		
		public override long GetItemId (int position)
		{
			return position;
		}
		
		public override int Count
		{
			get { return tasks.Count; }
		}
		
		public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
		{
			// Get our object for position
			var item = tasks[position];			

			//Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			// gives us some performance gains by not always inflating a new view
			// will sound familiar to MonoTouch developers with UITableViewCell.DequeueReusableCell()

			// TODO: use this code to populate the row, and remove the above view
			var view = (convertView ??
				context.LayoutInflater.Inflate(
					Android.Resource.Layout.SimpleListItemChecked,
					parent,
					false)) as CheckedTextView;
			view.SetText (item.Name==""?"<new task>":item.Name, TextView.BufferType.Normal);
			view.Checked = item.Done;


			//Finally return the view
			return view;
		}
	}
}