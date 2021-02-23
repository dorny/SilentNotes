using System;
using NUnit.Framework;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestFixture]
    public class NoteModelTest
    {
        [Test]
        public void LazyGenerationOfId()
        {
            NoteModel model = new NoteModel();
            Guid id1 = model.Id;
            Guid id2 = model.Id;

            Assert.AreNotEqual(Guid.Empty, model.Id); // Id is created when first accessed
            Assert.AreEqual(id1, id2); // Id does not change when accessed repeatedly
        }

        [Test]
        public void CloneCopiesAllProperties()
        {
            NoteModel note1 = new NoteModel
            {
                Id = Guid.NewGuid(),
                NoteType = NoteType.Checklist,
                HtmlContent = "<html>",
                BackgroundColorHex = "#000000",
                InRecyclingBin = true,
                CreatedAt = new DateTime(2000, 10, 22, 18, 55, 30),
                ModifiedAt = new DateTime(2001, 10, 22, 18, 55, 30),
                MaintainedAt = new DateTime(2002, 10, 22, 18, 55, 30),
                SafeId = new Guid("10000000000000000000000000000000"),
            };
            NoteModel note2 = note1.Clone();

            Assert.AreEqual(note1.Id, note2.Id);
            Assert.AreEqual(note1.NoteType, note2.NoteType);
            Assert.AreEqual(note1.HtmlContent, note2.HtmlContent);
            Assert.AreEqual(note1.BackgroundColorHex, note2.BackgroundColorHex);
            Assert.AreEqual(note1.InRecyclingBin, note2.InRecyclingBin);
            Assert.AreEqual(note1.CreatedAt, note2.CreatedAt);
            Assert.AreEqual(note1.ModifiedAt, note2.ModifiedAt);
            Assert.AreEqual(note1.MaintainedAt, note2.MaintainedAt);
            Assert.AreEqual(note1.SafeId, note2.SafeId);
        }

        [Test]
        public void Clone_CopiesTags()
        {
            NoteModel note1 = new NoteModel();
            NoteModel note2 = note1.Clone();

            Assert.IsFalse(note2.TagsSpecified);

            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            note1.Tags.Add(id1);
            note1.Tags.Add(id2);
            note2 = note1.Clone();

            Assert.IsTrue(note2.TagsSpecified);
            Assert.AreEqual(2, note2.Tags.Count);
            Assert.AreEqual(id1, note2.Tags[0]);
            Assert.AreEqual(id2, note2.Tags[1]);
        }
    }
}
