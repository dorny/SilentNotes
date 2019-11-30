﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using SilentNotes.Workers;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show a <see cref="NoteRepositoryViewModel"/> in an (Html)view.
    /// </summary>
    public class NoteRepositoryController : ControllerBase
    {
        private readonly IRazorViewService _viewContentService;
        private readonly IRazorViewService _viewStop;
        private NoteRepositoryViewModel _viewModel;
        private StopViewModel _stopViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryController"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public NoteRepositoryController(
            IRazorViewService viewService,
            IRazorViewService viewContentService,
            IRazorViewService stopService)
            : base(viewService)
        {
            _viewContentService = viewContentService;
            _viewStop = stopService;
        }

        /// <inheritdoc/>
        protected override void OverrideableDispose()
        {
            View.NavigationCompleted -= NavigationCompletedEventHandler;
            base.OverrideableDispose();
        }

        /// <inheritdoc/>
        protected override IViewModel GetViewModel()
        {
            if (_viewModel != null)
                return _viewModel;
            else
                return _stopViewModel;
        }

        /// <inheritdoc/>
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables)
        {
            base.ShowInView(htmlView, variables);
            View.NavigationCompleted += NavigationCompletedEventHandler;
            IRepositoryStorageService repositoryService = Ioc.GetOrCreate<IRepositoryStorageService>();

            RepositoryStorageLoadResult loadResult = repositoryService.LoadRepositoryOrDefault(out _);
            if (loadResult != RepositoryStorageLoadResult.InvalidRepository)
            {
                _viewModel = new NoteRepositoryViewModel(
                    Ioc.GetOrCreate<INavigationService>(),
                    Ioc.GetOrCreate<ILanguageService>(),
                    Ioc.GetOrCreate<ISvgIconService>(),
                    Ioc.GetOrCreate<IBaseUrlService>(),
                    Ioc.GetOrCreate<IStoryBoardService>(),
                    Ioc.GetOrCreate<IFeedbackService>(),
                    Ioc.GetOrCreate<ISettingsService>(),
                    Ioc.GetOrCreate<IThemeService>(),
                    Ioc.GetOrCreate<IEnvironmentService>(),
                    repositoryService);

                Bindings.BindCommand("AddNote", _viewModel.AddNoteCommand);
                Bindings.BindCommand("ShowNote", _viewModel.ShowNoteCommand);
                Bindings.BindCommand("DeleteNote", _viewModel.DeleteNoteCommand);
                Bindings.BindCommand("Synchronize", _viewModel.SynchronizeCommand);
                Bindings.BindCommand("ShowTransferCode", _viewModel.ShowTransferCodeCommand);
                Bindings.BindCommand("ShowRecycleBin", _viewModel.ShowRecycleBinCommand);
                Bindings.BindCommand("ShowSettings", _viewModel.ShowSettingsCommand);
                Bindings.BindCommand("ShowInfo", _viewModel.ShowInfoCommand);
                Bindings.BindCommand("OpenSafe", _viewModel.OpenSafeCommand);
                Bindings.BindCommand("CloseSafe", _viewModel.CloseSafeCommand);
                Bindings.BindCommand("ChangeSafePassword", _viewModel.ChangeSafePasswordCommand);
                Bindings.BindCommand("FilterButtonCancel", _viewModel.ClearFilterCommand);
                Bindings.BindCommand("Fab", _viewModel.AddNoteCommand);
                Bindings.BindText("TxtFilter", () => _viewModel.Filter, (value) => _viewModel.Filter = value, _viewModel, nameof(_viewModel.Filter), HtmlViewBindingMode.TwoWay);
                Bindings.BindGeneric(
                    (value) => SelectedNoteChangedEventHandler(value),
                    null,
                    () => _viewModel.SelectedNote,
                    null,
                    new HtmlViewBindingViewmodelNotifier(_viewModel, nameof(_viewModel.SelectedNote)),
                    HtmlViewBindingMode.OneWayToViewPlusOneTimeToView);
                Bindings.BindVisibility("FilterButtonMagnifier", () => string.IsNullOrEmpty(_viewModel.Filter), _viewModel, nameof(_viewModel.FilterButtonMagnifierVisible), HtmlViewBindingMode.OneWayToView);
                Bindings.BindVisibility("FilterButtonCancel", () => !string.IsNullOrEmpty(_viewModel.Filter), _viewModel, nameof(_viewModel.FilterButtonCancelVisible), HtmlViewBindingMode.OneWayToView);
                Bindings.BindGeneric<object>(
                    NotesChangedEventHandler,
                    null,
                    null,
                    null,
                    new HtmlViewBindingViewmodelNotifier(_viewModel, "Notes"),
                    HtmlViewBindingMode.OneWayToView);
                Bindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;

                // Load html page and content (notes)
                string html = _viewService.GenerateHtml(_viewModel);
                string htmlNotes = _viewContentService.GenerateHtml(_viewModel);
                html = html.Replace("<ul id=\"note-repository\"></ul>", htmlNotes); // Replace node "note-repository" with content
                View.LoadHtml(html);
            }
            else
            {
                // Show error message and stop loading the application
                _stopViewModel = new StopViewModel(
                    Ioc.GetOrCreate<INavigationService>(),
                    Ioc.GetOrCreate<ILanguageService>(),
                    Ioc.GetOrCreate<ISvgIconService>(),
                    Ioc.GetOrCreate<IBaseUrlService>());
                string html = _viewStop.GenerateHtml(_stopViewModel);
                View.LoadHtml(html);
            }
        }

        /// <inheritdoc/>
        protected override void SetHtmlViewBackgroundColor(IHtmlView htmlView)
        {
            IThemeService themeService = Ioc.GetOrCreate<IThemeService>();
            htmlView.SetBackgroundColor(ColorExtensions.HexToColor(themeService.SelectedTheme.ImageTint));
        }

        private void NavigationCompletedEventHandler(object sender, EventArgs e)
        {
            View.NavigationCompleted -= NavigationCompletedEventHandler;
            View.ExecuteJavaScript("makeSelectable(); makeSortable();");
        }

        private void NotesChangedEventHandler(object obj)
        {
            // Update the note list in the (HTML) view.
            string html = _viewContentService.GenerateHtml(_viewModel);
            View.ReplaceNode("note-repository", html);
            View.ExecuteJavaScript("makeSelectable(); makeSortable();");
        }

        private void SelectedNoteChangedEventHandler(NoteViewModel selectedNote)
        {
            string script = string.Format("selectNote('{0}');", selectedNote?.Id.ToString());
            View.ExecuteJavaScript(script);
        }

        private void SetVisibilityAddRemoveTresor(Guid noteId, bool isInSafe)
        {
            string bindingVisible = isInSafe ? "RemoveFromSafe" : "AddToSafe";
            string bindingInvisible = isInSafe ? "AddToSafe" : "RemoveFromSafe";
            string script = string.Format(
                "$(\"[data-note='{2}']\").children(\"[data-binding='{0}']\").show(); $(\"[data-note='{2}']\").children(\"[data-binding='{1}']\").hide();",
                bindingVisible,
                bindingInvisible,
                noteId.ToString());
            View.ExecuteJavaScript(script);
        }

        private void UnhandledViewBindingEventHandler(object sender, HtmlViewBindingNotifiedEventArgs e)
        {
            Guid noteId;
            switch (e.EventType?.ToLowerInvariant())
            {
                case "list-open":
                    noteId = new Guid(e.Parameters["data-note"]);
                    _viewModel.ShowNoteCommand.Execute(noteId);
                    break;
                case "list-select":
                    noteId = new Guid(e.Parameters["data-note"]);
                    _viewModel.SelectedNote = _viewModel.FilteredNotes.FirstOrDefault(item => item.Id == noteId);
                    break;
                case "list-orderchanged":
                    int oldIndex = int.Parse(e.Parameters["oldIndex"]);
                    int newIndex = int.Parse(e.Parameters["newIndex"]);
                    _viewModel.MoveNote(oldIndex, newIndex);
                    break;
            }
            switch (e.BindingName?.ToLowerInvariant())
            {
                case "addtosafe":
                    noteId = new Guid(e.Parameters["parent.data-note"]);
                    _viewModel.AddNoteToSafe(noteId);
                    SetVisibilityAddRemoveTresor(noteId, true);
                    break;
                case "removefromsafe":
                    noteId = new Guid(e.Parameters["parent.data-note"]);
                    _viewModel.RemoveNoteFromSafe(noteId);
                    SetVisibilityAddRemoveTresor(noteId, false);
                    break;
            }
        }
    }
}
