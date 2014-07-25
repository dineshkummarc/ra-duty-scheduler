﻿//Copyright (C) 2014  Dakota Kanner
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Duty_Schedule
{
    public class FileInputs
    {
        private string mDirectory;


        public FileInputs() 
        {
            mDirectory = Directory.GetCurrentDirectory();
        }

        public FileInputs(string directory)
        {
            mDirectory = directory;
        }

        // Gets the all the groups and names from a file
        public List<Person> GetGroups(string groupsFileName = "Groups.txt")
        {
            List<Person> pplLst = new List<Person>();

            //string fullFileLoc = mDirectory + "\\" + groupsFileName;

            if (File.Exists(groupsFileName))
            {
                StreamReader strRead = File.OpenText(groupsFileName);

                string lnStr = "";
                string currGroup = "";

                while ((lnStr = strRead.ReadLine()) != null)
                {
                    if (lnStr.Trim().Length > 0 && !char.IsWhiteSpace(lnStr[0]))
                    {
                        // If it isn't whitespace, it should be a group name

                        // Ignore comments in the file
                        if (lnStr.Length >= 2 && lnStr[0] != '#')
                        {
                            currGroup = lnStr.Trim();
                        }

                        // TODO: Search through all groups and add people as needed
                    }
                    else if(lnStr.Trim().Length > 0)
                    {
                        string datesRequestedOff = "";
                        // This should be a person so remove leading/ending whitespace and find where the name ends
                        string personName = lnStr.Trim();
                        int sepIndex = personName.IndexOf('-');

                        // Ignore comments in the file
                        if (personName[0] != '#') 
                        {
                            // If there is a range of dates requested off
                            if ( sepIndex > -1)
                            {
                                datesRequestedOff = personName.Substring(sepIndex);
                                personName = personName.Substring(0, sepIndex).Trim();

                                // Clean up the dates string
                                datesRequestedOff = datesRequestedOff.Trim('-').Trim();
                                List<DateTime> dateList = new List<DateTime>();

                                int commaIndex = datesRequestedOff.IndexOf(',');
                                while(commaIndex > -1)
                                {
                                    // Peel away the first date in the string and add it to the DateTime list
                                    string tempDateStr = datesRequestedOff.Substring(0, commaIndex).Trim(',').Trim();
                                    datesRequestedOff = datesRequestedOff.Substring(commaIndex).Trim(',').Trim(); 
                                    
                                    if (tempDateStr.Length > 0)
                                        dateList.Add(DateTime.Parse(tempDateStr));

                                    commaIndex = datesRequestedOff.IndexOf(',');
                                }
                                // Handle the last date in the string
                                if (datesRequestedOff.Length > 0)
                                    dateList.Add(DateTime.Parse(datesRequestedOff));
                                
                                // Actually add the person
                                bool foundPerson = false;
                                foreach (Person per in pplLst)
                                {
                                    if (per.mName == personName)
                                    {
                                        foundPerson = true;
                                        //If they aren't in this group already
                                        if(!per.mGroups.Contains(currGroup))
                                        {
                                            per.AddGroup(currGroup);
                                        }
                                        // If they don't have each date already
                                        foreach (DateTime dt in dateList)
                                        {
                                            if (!per.mDaysOffRequested.Contains(dt))
                                            {
                                                per.AddDayOffRequested(dt);
                                            }
                                        }
                                    }
                                }
                                if (!foundPerson)
                                    pplLst.Add(new Person(personName, currGroup, dateList));

                            }
                            else
                            {
                                personName = personName.Trim();
                                
                                // Actually add the person
                                bool foundPerson = false;
                                foreach (Person per in pplLst)
                                {
                                    if (per.mName == personName)
                                    {
                                        foundPerson = true;
                                        //If they aren't in this group already
                                        if (!per.mGroups.Contains(currGroup))
                                        {
                                            per.AddGroup(currGroup);
                                        }
                                    }
                                }
                                if (!foundPerson)
                                    pplLst.Add(new Person(personName, currGroup, new List<DateTime>() ) );
                            }
                        }
                    }
                }
            }

            if(pplLst.Count <= 0)
            {
                MessageBox.Show("No people in file " + groupsFileName, "Calendar Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            return pplLst;
        }   //End GetGroups(string groupsFileName = "Groups.txt")

        // Gets the all the groups and names from a file
        public DatesStruct GetDates(string dateFileName = "Dates.txt")
        {
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            List<DateTime> holidayList = new List<DateTime>();
            List<DateTime> breakList = new List<DateTime>();

            //string fullFileLoc = mDirectory + "\\" + dateFileName;

            if (File.Exists(dateFileName))
            {
                StreamReader strRead = File.OpenText(dateFileName);

                string lnStr = "";

                while ((lnStr = strRead.ReadLine()) != null)
                {
                    lnStr = lnStr.Trim();

                    if (lnStr.Length > 0 && lnStr[0] != '#')
                    {
                        // Start date = 's'
                        if (lnStr.ToLower()[0] == 's')
                        {
                            int sepIndex = lnStr.IndexOf('-');
                            if (sepIndex >= 0)
                            {
                                lnStr = lnStr.Substring(sepIndex + 1).Trim();
                                startDate = DateTime.Parse(lnStr);
                            }
                        }
                        // End date = 'e'
                        else if (lnStr.ToLower()[0] == 'e')
                        {
                            int sepIndex = lnStr.IndexOf('-');
                            if (sepIndex >= 0)
                            {
                                lnStr = lnStr.Substring(sepIndex + 1).Trim();
                                endDate = DateTime.Parse(lnStr);
                            }
                        }
                        // Holiday = 'h'
                        else if (lnStr.ToLower()[0] == 'h')
                        {
                            int sepIndex = lnStr.IndexOf('-');
                            if (sepIndex >= 0)
                            {
                                lnStr = lnStr.Substring(sepIndex + 1).Trim();

                                int commaIndex = lnStr.IndexOf(',');
                                while (commaIndex > -1)
                                {
                                    // Peel away the first date in the string and add it to the DateTime list
                                    string tempDateStr = lnStr.Substring(0, commaIndex).Trim(',').Trim();
                                    lnStr = lnStr.Substring(commaIndex).Trim(',').Trim();

                                    if (tempDateStr.Length > 0)
                                        holidayList.Add(DateTime.Parse(tempDateStr));

                                    commaIndex = lnStr.IndexOf(',');
                                }
                                // Handle the last date in the string
                                if (lnStr.Length > 0)
                                    holidayList.Add(DateTime.Parse(lnStr));
                            }
                        }
                        else if (lnStr.ToLower()[0] == 'b')
                        {
                            int sepIndex = lnStr.IndexOf('-');
                            if (sepIndex >= 0)
                            {
                                lnStr = lnStr.Substring(sepIndex + 1).Trim();

                                int commaIndex = lnStr.IndexOf(',');
                                while (commaIndex > -1)
                                {
                                    // Peel away the first date in the string and add it to the DateTime list
                                    string tempDateStr = lnStr.Substring(0, commaIndex).Trim(',').Trim();
                                    lnStr = lnStr.Substring(commaIndex).Trim(',').Trim();

                                    if (tempDateStr.Length > 0)
                                        breakList.Add(DateTime.Parse(tempDateStr));

                                    commaIndex = lnStr.IndexOf(',');
                                }
                                // Handle the last date in the string
                                if (lnStr.Length > 0)
                                    breakList.Add(DateTime.Parse(lnStr));
                            }
                        }
                    }
                }
            }

            if (startDate == new DateTime())
            {
                MessageBox.Show("No start date found in file " + dateFileName, "Calendar Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (endDate == new DateTime())
            {
                MessageBox.Show("No end date found in file " + dateFileName, "Calendar Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            DatesStruct ds = new DatesStruct();
            ds.startDate = startDate;
            ds.endDate = endDate;
            ds.holidayList = holidayList;
            ds.breakList = breakList;

            return ds;
        }   //End GetGroups(string groupsFileName = "Groups.txt")

    }
}
