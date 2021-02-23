﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SilentNotes.Models
{
    /// <summary>
    /// Serializeable model of a note repository.
    /// Whenever changes to this model are done, take care of the <see cref="NewestSupportedRevision"/> constant.
    /// </summary>
    [XmlRoot(ElementName = "silentnotes")]
    public class NoteRepositoryModel
    {
        /// <summary>The highest revision of the repository which can be handled by this application.</summary>
        // todo: increase revision
        public const int NewestSupportedRevision = 4;
        private Guid _id;
        private NoteListModel _notes;
        private List<Guid> _deletedNotes;
        private SafeListModel _safes;
        private TagListModel _tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryModel"/> class.
        /// </summary>
        public NoteRepositoryModel()
        {
            OrderModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the id of the repository.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get { return (_id != Guid.Empty) ? _id : (_id = Guid.NewGuid()); }
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
            get { return _notes ?? (_notes = new NoteListModel()); }
            set { _notes = value; }
        }

        /// <summary>
        /// Gets or sets a list of ids of deleted notes.
        /// </summary>
        [XmlArray("deleted_notes")]
        [XmlArrayItem("deleted_note")]
        public List<Guid> DeletedNotes
        {
            get { return _deletedNotes ?? (_deletedNotes = new List<Guid>()); }
            set { _deletedNotes = value; }
        }

        /// <summary>
        /// Gets or sets a list of safes which are used to encrypt notes. Ideally at most one safe
        /// is used, but because of synchronisation between several devices, there can be more than
        /// one safe.
        /// </summary>
        [XmlArray("safes")]
        [XmlArrayItem("safe")]
        public SafeListModel Safes
        {
            get { return _safes ?? (_safes = new SafeListModel()); }
            set { _safes = value; }
        }

        /// <summary>
        /// Gets or sets a list of tags which are used to order the notes.
        /// </summary>
        [XmlArray("tags")]
        [XmlArrayItem("tag")]
        public TagListModel Tags
        {
            get { return _tags ?? (_tags = new TagListModel()); }
            set { _tags = value; }
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
                hashCode = (hashCode * 397) ^ OrderModifiedAt.GetHashCode();
                foreach (NoteModel note in Notes)
                {
                    hashCode = (hashCode * 397) ^ note.ModifiedAt.GetHashCode();
                    if (note.MaintainedAt != null)
                        hashCode = (hashCode * 397) ^ note.MaintainedAt.GetHashCode();
                }
                foreach (Guid deletedNote in DeletedNotes)
                {
                    hashCode = (hashCode * 397) ^ deletedNote.GetHashCode();
                }
                foreach (SafeModel safe in Safes)
                {
                    hashCode = (hashCode * 397) ^ safe.ModifiedAt.GetHashCode();
                    if (safe.MaintainedAt != null)
                        hashCode = (hashCode * 397) ^ safe.MaintainedAt.GetHashCode();
                }
                foreach (TagModel tag in Tags)
                {
                    hashCode = (hashCode * 397) ^ tag.ModifiedAt.GetHashCode();
                }
                return hashCode;
            }
        }

        /// <summary>
        /// Clears all the MaintainedAt properties if they are obsolete, because the object was
        /// modified later.
        /// </summary>
        public void ClearMaintainedAtIfObsolete()
        {
            foreach (NoteModel note in Notes)
                note.ClearMaintainedAtIfObsolete();
            foreach (SafeModel safe in Safes)
                safe.ClearMaintainedAtIfObsolete();
        }

        /// <summary>
        /// Removes all safes, which are not used by any note anymore.
        /// </summary>
        public void RemoveUnusedSafes()
        {
            List<Guid> usedSafeIds = Notes.Where(note => note.SafeId != null).Select(note => note.SafeId.Value).ToList();
            Safes.RemoveAll(safe => !usedSafeIds.Contains(safe.Id));
        }

        /// <summary>
        /// Removes all tags, which are not used by any note anymore.
        /// </summary>
        public void RemoveUnusedTags()
        {
            HashSet<Guid> usedTagIds = new HashSet<Guid>();
            foreach (NoteModel note in Notes)
            {
                if (note.TagsSpecified)
                {
                    foreach (Guid tagId in note.Tags)
                    {
                        if (!usedTagIds.Contains(tagId))
                            usedTagIds.Add(tagId);
                    }
                }
            }
            Tags.RemoveAll(tag => !usedTagIds.Contains(tag.Id));
        }
    }
}
