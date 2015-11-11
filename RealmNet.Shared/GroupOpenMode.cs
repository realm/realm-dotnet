/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealmNet
{
    /// <summary>
    ///  //Open in read-only mode. Fail if the file does not already exist.
    ///  mode_ReadOnly,
    ///  //Open in read/write mode. Create the file if it doesn't exist.
    ///  mode_ReadWrite,
    ///  //Open in read/write mode. Fail if the file does not already exist.
    ///  mode_ReadWriteNoCreate
    /// </summary>        
    public enum GroupOpenMode    //nested type inside Group, as in core. CA10344 warning ignored.
    {

        /// <summary>
        /// Open in read-only mode. Fail if the file does not already exist.
        /// </summary>
        ModeReadOnly,

        /// <summary>
        /// Open in read/write mode. Create the file if it doesn't exist.
        /// </summary>

        ModeReadWrite,

        /// <summary>
        /// Open in read/write mode. Fail if the file does not already exist.
        /// </summary>

        ModeReadWriteNoCreate
    }
}
