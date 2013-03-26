Imports Uiuc.Library.Premis
Imports System.IO
Imports System.Text.RegularExpressions

''' <summary>
''' Represents the pages of a Full DSD Book model, se https://wiki.cites.uiuc.edu/wiki/display/LibraryDigitalPreservation/PREMIS+Package+Structure
''' </summary>
''' <remarks></remarks>
Public Class MedusaFullDsdBookPages

  Private _container As PremisContainer ' the PremisContainer that will contain all the premis page entities

  Private _premisPageRoot As PremisObject   'represents the aggregation of all pages

  Private _premisPages As List(Of PremisObject) 'ordered list of all pages


  'each page can have both a production master image and a screen size image.   The key for these dictionaries is the LOCAL identifier of the PremisObject representing the page
  Private _originalPageImages As Dictionary(Of PremisIdentifier, PremisObject)
  Private _derivativePageImages As Dictionary(Of PremisIdentifier, PremisObject)

  Public Sub New(pContainer As PremisContainer, pRepresentation As PremisObject)
    _container = pContainer
    _premisPageRoot = New PremisObject("LOCAL", _container.NextID, PremisObjectCategory.Representation)
    pRepresentation.RelateToObject("PAGED_TEXT_ASSET", "PAGES", _premisPageRoot)
    _premisPageRoot.RelateToObject("PAGED_TEXT_ASSET", "PARENT", pRepresentation)
    _container.Objects.Add(_premisPageRoot)

    _premisPages = New List(Of PremisObject)

    _originalPageImages = New Dictionary(Of PremisIdentifier, PremisObject)
    _derivativePageImages = New Dictionary(Of PremisIdentifier, PremisObject)
  End Sub

  ''' <summary>
  ''' Add a page image to the list of pages
  ''' </summary>
  ''' <param name="premisPageImgObj">The PREMIS file object representing the page image</param>
  ''' <remarks></remarks>
  Public Sub AddPage(premisPageImgObj As PremisObject)
    Dim origName As String = premisPageImgObj.OriginalName
    Dim fileRoot As String = Path.GetFileNameWithoutExtension(origName)
    Dim isDerivedContent As Boolean = False

    'determine if this page is master or screen size image
    If Regex.IsMatch(origName, MedusaAppSettings.Settings.DerivativeContentFileRegex, RegexOptions.IgnoreCase) Then
      isDerivedContent = True
    ElseIf Regex.IsMatch(origName, MedusaAppSettings.Settings.OriginalContentFileRegex, RegexOptions.IgnoreCase) Then
      isDerivedContent = False
    Else
      Throw New MedusaException(String.Format("File '{0}' does not appear to be a page image", origName))
    End If

    'see if page has already been created and create if needed
    Dim pgs = _premisPages.Where(Function(p) p.GetFilenameIdentifier.IdentifierValue.ToLowerInvariant = fileRoot.ToLowerInvariant) 'probably rather inefficient search for long list of pages

    Dim premisPage As PremisObject
    Dim Pid As PremisIdentifier = Nothing
    If pgs.Count = 0 Then
      Pid = _container.NextLocalIdentifier
      premisPage = New PremisObject(Pid.IdentifierType, Pid.IdentifierValue, PremisObjectCategory.Representation)
      Dim fnameId As PremisIdentifier = New PremisIdentifier("FILENAME", Path.GetFileNameWithoutExtension(premisPageImgObj.OriginalName))
      premisPage.ObjectIdentifiers.Add(fnameId)
      _container.Objects.Add(premisPage)
      Dim lastPg As PremisObject = _premisPages.LastOrDefault
      _premisPages.Add(premisPage)

      'link the new page object to its siblings and parent pages object
      If lastPg Is Nothing Then 'this is first page being added
        _premisPageRoot.RelateToObject("BASIC_COMPOUND_ASSET", "FIRST_CHILD", premisPage)
      Else ' this is a subsequernt page to link to previous sibling
        lastPg.RelateToObject("BASIC_COMPOUND_ASSET", "NEXT_SIBLING", premisPage)
        premisPage.RelateToObject("BASIC_COMPOUND_ASSET", "PREVIOUS_SIBLING", lastPg)
      End If
      'all pages are linked to parent pages object
      premisPage.RelateToObject("BASIC_COMPOUND_ASSET", "PARENT", _premisPageRoot)

    ElseIf pgs.Count = 1 Then
      premisPage = pgs.Single
      Pid = New PremisIdentifier("LOCAL", premisPage.LocalIdentifierValue)
    Else
      Throw New MedusaException(String.Format("Unexpected duplicates pages found for file '{0}'", origName))
    End If

    'add to appropriate dictionary and make sure it links to its parent pages object
    If isDerivedContent Then
      _derivativePageImages.Add(Pid, premisPageImgObj)
      premisPage.RelateToObject("BASIC_IMAGE_ASSET", "SCREEN_SIZE", premisPageImgObj)
    Else
      _originalPageImages.Add(Pid, premisPageImgObj)
      premisPage.RelateToObject("BASIC_IMAGE_ASSET", "PRODUCTION_MASTER", premisPageImgObj)
    End If
    premisPageImgObj.RelateToObject("BASIC_IMAGE_ASSET", "PARENT", premisPage)


  End Sub


End Class
