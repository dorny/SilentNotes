// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using SilentNotes.Models;

namespace SilentNotes.Models
{
    /// <summary>
    /// List of tags.
    /// </summary>
    public class TagListModel : List<TagModel>
    {
        /// <summary>
        /// Searches for a tag with a given id in the repository and returns the found tag.
        /// </summary>
        /// <param name="id">Search for the tag with this id.</param>
        /// <returns>Found tag, or null if no such tag exists.</returns>
        public TagModel FindById(Guid id)
        {
            return Find(item => item.Id == id);
        }
    }
}
