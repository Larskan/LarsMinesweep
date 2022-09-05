using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarsMinesweep.ViewModel
{
    public class Tiles
    {
        #region variables
        private bool isItAMine; //True and False checks if button/tile clicked contains a mine
        private bool isItFlagged; //True and False checks if button/tile clicked has been flagged as a mine
        private bool isItCleared; //True and False checks if button/tile has been clicked or cleared for mines
        private int column; //Is added to be able to have several tiers of difficulty
        private int row; //Is added to be able to have several tiers of difficulty
        #endregion

        public Tiles()
        {
            //Is added to the Grid to be able to have several tiers of difficulty, referenced in creation of grid
        }

        #region getters and setters for the variables
        public int Row
        {
            get => row;
            set => row = value;        
        }

        public int Column
        {
            get => column;
            set => column = value;
        }

        public bool IsItAMine
        {
            get => isItAMine;
            set => isItAMine = value;
        }

        public bool IsItFlagged
        {
            get => isItFlagged; 
            set => isItFlagged = value;
        }

        public bool IsItCleared
        {
            get => isItCleared;
            set => isItCleared = value;
        }
        #endregion

        public override string ToString()
        {
            //the Row, Column, IsItCleared, IsItAMine and IsItFlagged grabs the settes and getters of each
            //the $ seperates the strings from the setters/getters, without it, the entire thing would be a string
            return $"This tile is on Row: {Row}, Column: {Column}, Cleared: {IsItCleared}, IsItAMine: {IsItAMine}, IsItFlagged: {IsItFlagged}";
        }


    }

}
