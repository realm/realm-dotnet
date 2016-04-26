using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Tasky.Shared;
using TaskyAndroid;
using Realms;

namespace TaskyAndroid.Screens 
{
    /// <summary>
    /// View/edit a Task
    /// </summary>
    [Activity (Label = "Tasky")]			
    public class TodoItemScreen : Activity 
    {
        TodoItem task;
        Button cancelDeleteButton;
        EditText notesTextEdit;
        EditText nameTextEdit;
        Button saveButton;
        CheckBox doneCheckbox;
        Transaction transaction;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            
            transaction = TodoItemManager.BeginTransaction();

            var taskID = Intent.GetStringExtra("TaskID");
            if (taskID == null)
                task = TodoItemManager.CreateTodoItem();
            else
                task = TodoItemManager.GetTask(taskID);
            
            // set our layout to be the home screen
            SetContentView(Resource.Layout.TaskDetails);
            nameTextEdit = FindViewById<EditText>(Resource.Id.NameText);
            notesTextEdit = FindViewById<EditText>(Resource.Id.NotesText);
            saveButton = FindViewById<Button>(Resource.Id.SaveButton);

            // TODO: find the Checkbox control and set the value
            doneCheckbox = FindViewById<CheckBox>(Resource.Id.chkDone);
            doneCheckbox.Checked = task.Done;

            // find all our controls
            cancelDeleteButton = FindViewById<Button>(Resource.Id.CancelDeleteButton);
            
            // set the cancel delete based on whether or not it's an existing task
            cancelDeleteButton.Text = "Delete";
            
            nameTextEdit.Text = task.Name; 
            notesTextEdit.Text = task.Notes;

            // button clicks 
            cancelDeleteButton.Click += (sender, e) => { CancelDelete(); };
            saveButton.Click += (sender, e) => { Save(); };
        }

        void Save()
        {
            task.Name = nameTextEdit.Text;
            task.Notes = notesTextEdit.Text;
            task.Done = doneCheckbox.Checked;

            TodoItemManager.SaveTask(task);
            transaction.Commit();
            Finish();
        }
        
        void CancelDelete()
        {
            if (task.ID != null) {
                TodoItemManager.DeleteTask(task.ID);
            }
            transaction.Commit();
            Finish();
        }
    }
}