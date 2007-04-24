using System;
using JMS.DVB;
using System.Text;
using JMS.DVB.EPG;
using System.Threading;
using JMS.DVB.EPG.Tables;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// 
    /// </summary>
	public abstract class ExtendedSITScanner<TableType> : SITScanner where TableType : Table
	{
        /// <summary>
        /// All table fragments we collected.
        /// </summary>
        public TableType[] TableFragments = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device">The hardware abstraction to use</param>
        /// <param name="pid">The transport stream identifier of the PMT.</param>
        /*protected ExtendedSITScanner(IDeviceProvider device, ushort pid)
            : base(device, pid)
        {
        }*/

        /// <summary>
        /// See if we are valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                // Not possible
                if ((null == TableFragments) || (TableFragments.Length < 1)) return false;

                // Test all
                foreach (TableType table in TableFragments)
                    if (null == table)
                        return false;

                // We are
                return true;
            }
        }

        /// <summary>
        /// Process a SI table.
        /// </summary>
        /// <param name="table">The SI table.</param>
        /// <returns>Set if all PAT entries are parsed.</returns>
        protected override bool OnTableFound(Table table)
        {
            // Load the table
            TableType typedTable = table as TableType;

            // Verify
            if (null == typedTable) return false;

            // Must reset
            if (!typedTable.IsCurrent)
            {
                // Restart
                TableFragments = null;

                // Done
                return false;
            }

            // Create the resultant table
            if ((null == TableFragments) || (0 == typedTable.SectionNumber))
            {
                // Wait for the first section
                if (0 != typedTable.SectionNumber) return false;

                // Create
                TableFragments = new TableType[typedTable.LastSectionNumber + 1];
            }

            // Already set - must restart
            if (null != TableFragments[typedTable.SectionNumber])
            {
                // Restart
                TableFragments = null;

                // Done
                return false;
            }

            // Remember
            TableFragments[typedTable.SectionNumber] = typedTable;

            // See if this is it
            for (int i = TableFragments.Length; i-- > 0; )
                if (null == TableFragments[i])
                    return false;

            // Signal the event
            return true;
        }
    }
}
