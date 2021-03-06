﻿namespace UglyToad.PdfPig.Content
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Filters;
    using Graphics;
    using Graphics.Operations;
    using Tokenization.Scanner;
    using XObjects;

    /// <summary>
    /// Wraps content parsed from a page content stream for access.
    /// </summary>
    /// <remarks>
    /// This should contain a replayable stack of drawing instructions for page content
    /// from a content stream in addition to lazily evaluated state such as text on the page or images.
    /// </remarks>
    internal class PageContent
    {
        private readonly IReadOnlyList<Union<XObjectContentRecord, InlineImage>> images;
        private readonly IReadOnlyList<MarkedContentElement> markedContents;
        private readonly IPdfTokenScanner pdfScanner;
        private readonly IFilterProvider filterProvider;
        private readonly IResourceStore resourceStore;

        internal IReadOnlyList<IGraphicsStateOperation> GraphicsStateOperations { get; }

        public IReadOnlyList<Letter> Letters { get; }

        public IReadOnlyList<PdfPath> Paths { get; }

        public int NumberOfImages => images.Count;

        internal PageContent(IReadOnlyList<IGraphicsStateOperation> graphicsStateOperations, IReadOnlyList<Letter> letters,
            IReadOnlyList<PdfPath> paths,
            IReadOnlyList<Union<XObjectContentRecord, InlineImage>> images,
            IReadOnlyList<MarkedContentElement> markedContents,
            IPdfTokenScanner pdfScanner,
            IFilterProvider filterProvider,
            IResourceStore resourceStore)
        {
            GraphicsStateOperations = graphicsStateOperations;
            Letters = letters;
            Paths = paths;
            this.images = images;
            this.markedContents = markedContents;
            this.pdfScanner = pdfScanner ?? throw new ArgumentNullException(nameof(pdfScanner));
            this.filterProvider = filterProvider ?? throw new ArgumentNullException(nameof(filterProvider));
            this.resourceStore = resourceStore ?? throw new ArgumentNullException(nameof(resourceStore));
        }

        public IEnumerable<IPdfImage> GetImages()
        {
            foreach (var image in images)
            {
                var result = image.Match<IPdfImage>(x => XObjectFactory.ReadImage(x, pdfScanner, filterProvider, resourceStore),
                    x => x);

                yield return result;
            }
        }

        public IReadOnlyList<MarkedContentElement> GetMarkedContents() => markedContents;
    }
}
