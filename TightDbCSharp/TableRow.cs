namespace TightDbCSharp
{
    //methods here are those that only work on a row in a table, not on a row in a tableview. They are here, to make sure the user cannot call them on the Row class
    public class TableRow:Row
    {
        internal TableRow(Table owner, long row): base(owner, row)
        {
            OwnerTable = owner;
        }

        internal Table OwnerTable { get; set; }//same as owner, but this is typed as a table (saving us typecasts and checks on access)

    }
}
