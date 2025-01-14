﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Diagnostics;

namespace Microsoft.SemanticKernel.Orchestration;

/// <summary>
/// Semantic Kernel context.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class SKContext
{
    /// <summary>
    /// Print the processed input, aka the current data after any processing occurred.
    /// </summary>
    /// <returns>Processed input, aka result</returns>
    public string Result => this.Variables.ToString();

    /// <summary>
    /// When a prompt is processed, aka the current data after any model results processing occurred.
    /// (One prompt can have multiple results).
    /// </summary>
    [Obsolete($"ModelResults are now part of {nameof(FunctionResult.Metadata)} property. Use 'ModelResults' key or available extension methods to get model results.")]
    public IReadOnlyCollection<ModelResult> ModelResults => Array.Empty<ModelResult>();

    /// <summary>
    /// The culture currently associated with this context.
    /// </summary>
    public CultureInfo Culture
    {
        get => this._culture;
        set => this._culture = value ?? CultureInfo.CurrentCulture;
    }

    /// <summary>
    /// User variables
    /// </summary>
    public ContextVariables Variables { get; }

    /// <summary>
    /// Read only functions collection
    /// </summary>
    public IReadOnlyFunctionCollection Functions { get; }

    /// <summary>
    /// App logger
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Kernel context reference
    /// </summary>
    public IKernel Kernel => this.GetKernelContext();

    /// <summary>
    /// Spawns the kernel for the context.
    /// </summary>
    /// <remarks>
    /// The kernel context is a lightweight instance of the main kernel with its services.
    /// </remarks>
    /// <returns>Kernel reference</returns>
    private IKernel GetKernelContext()
        => this._originalKernel; // TODO: Clone a lightweight kernel instead of returning the same instance

    /// <summary>
    /// Constructor for the context.
    /// </summary>
    /// <param name="kernel">Kernel reference</param>
    /// <param name="variables">Context variables to include in context.</param>
    /// <param name="functions">Functions to include in context.</param>
    public SKContext(
        IKernel kernel,
        ContextVariables? variables = null,
        IReadOnlyFunctionCollection? functions = null)
    {
        Verify.NotNull(kernel, nameof(kernel));

        this._originalKernel = kernel;
        this.Variables = variables ?? new();
        this.Functions = functions ?? NullReadOnlyFunctionCollection.Instance;
        this.LoggerFactory = kernel.LoggerFactory;
        this._culture = CultureInfo.CurrentCulture;
    }

    /// <summary>
    /// Constructor for the context.
    /// </summary>
    /// <param name="kernel">Kernel instance parameter</param>
    /// <param name="variables">Context variables to include in context.</param>
    public SKContext(
        IKernel kernel,
        ContextVariables? variables = null) : this(kernel, variables, kernel.Functions)
    {
    }

    /// <summary>
    /// Constructor for the context.
    /// </summary>
    /// <param name="kernel">Kernel instance parameter</param>
    /// <param name="functions">Functions to include in context.</param>
    public SKContext(
        IKernel kernel,
        IReadOnlyFunctionCollection? functions = null) : this(kernel, null, functions)
    {
    }

    /// <summary>
    /// Constructor for the context.
    /// </summary>
    /// <param name="kernel">Kernel instance parameter</param>
    public SKContext(IKernel kernel) : this(kernel, null, kernel.Functions)
    {
    }

    /// <summary>
    /// Print the processed input, aka the current data after any processing occurred.
    /// </summary>
    /// <returns>Processed input, aka result.</returns>
    public override string ToString()
    {
        return this.Result;
    }

    /// <summary>
    /// Create a clone of the current context, using the same kernel references (memory, functions, logger)
    /// and a new set variables, so that variables can be modified without affecting the original context.
    /// </summary>
    /// <returns>A new context copied from the current one</returns>
    public SKContext Clone()
    {
        return new SKContext(
            kernel: this._originalKernel,
            variables: this.Variables.Clone())
        {
            Culture = this.Culture,
        };
    }

    /// <summary>
    /// The culture currently associated with this context.
    /// </summary>
    private CultureInfo _culture;

    /// <summary>
    /// Kernel instance reference for this context.
    /// </summary>
    private readonly IKernel _originalKernel;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            string display = this.Variables.DebuggerDisplay;

            if (this.Functions is IReadOnlyFunctionCollection functions)
            {
                var view = functions.GetFunctionViews();
                display += $", Functions = {view.Count}";
            }

            display += $", Culture = {this.Culture.EnglishName}";

            return display;
        }
    }
}
