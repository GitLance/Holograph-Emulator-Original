using System;

using Holo.Managers;

namespace Holo.Virtual.Rooms.Items
{
    /// <summary>
    /// Represents a virtual flooritem in a virtual room.
    /// </summary>
    public class floorItem
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
        /// The X position of the item in the virtual room.
        /// </summary>
        internal int X;
        /// <summary>
        /// The Y position of the item in the virtual room.
        /// </summary>
        internal int Y;
        /// <summary>
        /// The rotation of the item in the virtual room.
        /// </summary>
        internal byte Z;
        /// <summary>
        /// The height of the item in the virtual room.
        /// </summary>
        internal double H;
        /// <summary>
        /// Optional. The variable/status of the item.
        /// </summary>
        internal string Var;
        /// <summary>
        /// Initializes a new instance of a virtual floor item in a virtual room.
        /// </summary>
        /// <param name="ID">The ID of this item.</param>
        /// <param name="tID">The template ID of this item.</param>
        /// <param name="X">The X position of this item.</param>
        /// <param name="Y">The Y position of this item.</param>
        /// <param name="Z">The Z rotation of this item.</param>
        /// <param name="H">The height on which this item is located.</param>
        /// <param name="Var">The variable of this item. [optional]</param>
        public floorItem(int ID, int tID, int X, int Y, int Z, double H, string Var)
        {
            this.ID = ID;
            this.templateID = tID;
            this.X = X;
            this.Y = Y;
            this.Z = Convert.ToByte(Z);
            this.H = H;
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
            catalogueManager.itemTemplate Template = catalogueManager.getTemplate(templateID);
            if (Template.Sprite == "song_disk")
                return ID.ToString() + Convert.ToChar(2) + Template.Sprite + Convert.ToChar(2) + Encoding.encodeVL64(X) + Encoding.encodeVL64(Y) + Encoding.encodeVL64(Template.Length) + Encoding.encodeVL64(Template.Width) + Encoding.encodeVL64(Z) + H.ToString().Replace(",", ".") + Convert.ToChar(2) + Template.Colour + Convert.ToChar(2) + Convert.ToChar(2) + Var + Convert.ToChar(2);
            else
                return ID.ToString() + Convert.ToChar(2) + Template.Sprite + Convert.ToChar(2) + Encoding.encodeVL64(X) + Encoding.encodeVL64(Y) + Encoding.encodeVL64(Template.Length) + Encoding.encodeVL64(Template.Width) + Encoding.encodeVL64(Z) + H.ToString().Replace(",",".") + Convert.ToChar(2) + Template.Colour + Convert.ToChar(2) + Convert.ToChar(2) + "H" + Var + Convert.ToChar(2);   
        }
    }
}
