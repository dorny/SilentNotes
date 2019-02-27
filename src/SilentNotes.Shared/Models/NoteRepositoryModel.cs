﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
    /// <summary>
    /// Serializeable model of a note repository.
    /// Whenever changes to this model are done, take care of the <see cref="SupportedRevision"/> property.
    /// </summary>
    [XmlRoot(ElementName = "silentnotes")]
    public class NoteRepositoryModel
    {
        /// <summary>The highest revision of the repository which can be handled by this application.</summary>
        public const int NewestSupportedRevision = 2;
        private Guid _id;
        private NoteListModel _notes;
        private List<Guid> _deletedNotes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryModel"/> class.
        /// </summary>
        public NoteRepositoryModel()
        {
            _id = Guid.Empty;
            OrderModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the id of the repository.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get
            {
                if (Guid.Empty == _id)
                    _id = Guid.NewGuid();
                return _id;
            }

            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the revision, which was used to create the repository.
        /// </summary>
        [XmlAttribute(AttributeName = "revision")]
        public int Revision { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the order of the notes was last changed.
        /// </summary>
        [XmlAttribute(AttributeName = "order_modified_at")]
        public DateTime OrderModifiedAt { get; set; }

        /// <summary>
        /// Sets the <see cref="OrderModifiedAt"/> property to the current UTC time.
        /// </summary>
        public void RefreshOrderModifiedAt()
        {
            OrderModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets a list of notes.
        /// </summary>
        [XmlArray("notes")]
        [XmlArrayItem("note")]
        public NoteListModel Notes
        {
            get { return LazyCreator.GetOrCreate(ref _notes); }
            set { _notes = value; }
        }

        /// <summary>
        /// Gets or sets a list of ids of deleted notes.
        /// </summary>
        [XmlArray("deleted_notes")]
        [XmlArrayItem("deleted_note")]
        public List<Guid> DeletedNotes
        {
            get { return LazyCreator.GetOrCreate(ref _deletedNotes); }
            set { _deletedNotes = value; }
        }

        /// <summary>
        /// Gets a fingerprint of the current repository, which can be used to determine, whether
        /// two repositories are different, or if a repository was modified in the meantime.
        /// Equal fingerprints mean unchanged repositories, different fingerprints indicate
        /// modifications in the repository.
        /// </summary>
        /// <remarks>
        /// This method is optimized for speed, so it does not consider the whole content of the
        /// repository, instead it uses the timestamps which would be used when merging.
        /// </remarks>
        /// <returns>A fingerprint representing the modification state.</returns>
        public long GetModificationFingerprint()
        {
            unchecked
            {
                long hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Revision;
                hashCode = (hashCode * 397) ^ (OrderModifiedAt != null ? OrderModifiedAt.GetHashCode() : 0);
                foreach (NoteModel note in Notes)
                {
                    hashCode = (hashCode * 397) ^ note.ModifiedAt.GetHashCode();
                }
                foreach (Guid deletedNote in DeletedNotes)
                {
                    hashCode = (hashCode * 397) ^ deletedNote.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}
