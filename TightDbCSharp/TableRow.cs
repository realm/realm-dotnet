namespace TightDbCSharp
{
    //methods here are those that only work on a row in a table, not on a row in a tableview. They are here, to make sure the user cannot call them on the Row class
    /// <summary>
    /// Represents a Row gotten from a Table.
    /// (not from a tableView).
    /// This class might get depricated later on,
    /// if possible use Row instead.
    /// </summary>
    public class TableRow:Row
    {        
        internal TableRow(Table owner, long row): base(owner, row)
        {
            OwnerTable = owner;
        }
        
        /// <summary>
        /// The Table containing the row that this row represents
        /// </summary>
        public Table OwnerTable { get; private set; }//same as owner, but this is typed as a table (saving us typecasts and checks on access)
        
    }
}

