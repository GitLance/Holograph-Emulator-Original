using System;

using Holo.Managers;

namespace Holo.Virtual.Rooms.Items
{
    /// <summary>
    /// Represents a virtual wallitem in a virtual room.
    /// </summary>
    public class wallItem
    {
        /// <summary>
        /// The ID of the item.
        /// </summary>
        internal int ID;
        /// <summary>
        /// The template ID of the item.
        /// </summary>
        internal int templateID;
        /// <summary>
        /// The position of the item on the wall of the virtual room.
        /// </summary>
        internal string wallPosition;
        /// <summary>
        /// Optional. The variable/status of the item.
        /// </summary>
        internal string Var;

        /// <summary>
        /// Initializes a new instance of a virtual wallitem in a virtual room.
        /// </summary>
        /// <param name="ID">The ID of this item.</param>
        /// <param name="tID">The template ID of this item.</param>
        /// <param name="Wallposition">The wallposition of this item. [!Rabbit format]</param>
        /// <param name="Var">The variable of this item. [optional, if not supplied then sprite color will be set]</param>
        public wallItem(int ID, int tID, string wallPosition, string Var)
        {
            this.ID = ID;
            this.templateID = tID;
            this.wallPosition = wallPosition;
            if(Var == "")
                this.Var = catalogueManager.getTemplate(tID).Colour;
            else
                this.Var = Var;
        }
        /// <summary>
        /// Returns the sprite name of this item by accessing catalogueManager.itemTemplate with the template ID.
        /// </summary>
        internal string Sprite
        {
            get
            {
                return catalogueManager.getTemplate(templateID).Sprite;
            }
        }
        /// <summary>
        /// Returns the item string of this item.
        /// </summary>
        public override string ToString()
        {
            return ID + Convert.ToChar(9).ToString() + Sprite + Convert.ToChar(9) + " " + Convert.ToChar(9) + wallPosition + Convert.ToChar(9) + Var;
        }
    }
}
