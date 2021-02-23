// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
    /// <summary>
    /// Tags can be attached to a note, and can build a flexible folder/grouping system. In contrast
    /// to folders, the notes can have a m:n relation to the tags. We keep the tag as entity, so
    /// it is possible to rename a tag for all notes, even if synced across devices.
    /// </summary>
    public class TagModel
    {
        private Guid _id;

        /// <summary>
        /// Makes <paramref name="target"/> a deep copy of the tag.
        /// </summary>
        /// <param name="target">Copy all properties to this tag.</param>
        public void CloneTo(TagModel target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.Id = this.Id;
            target.Title = this.Title;
            target.ModifiedAt = this.ModifiedAt;
        }

        /// <summary>
        /// Gets or sets the id of the tag.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get { return (_id != Guid.Empty) ? _id : (_id = Guid.NewGuid()); }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the title of the tag.
        /// </summary>
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the tag was last updated.
        /// </summary>
        [XmlAttribute(AttributeName = "modified_at")]
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Sets the <see cref="ModifiedAt"/> property to the current UTC time.
        /// </summary>
        public void RefreshModifiedAt()
        {
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Makes a deep copy of the tag.
        /// </summary>
        /// <returns>Copy of the tag.</returns>
        public TagModel Clone()
        {
            TagModel result = new TagModel();
            CloneTo(result);
            return result;
        }
    }
}
