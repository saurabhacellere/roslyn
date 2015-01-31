// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel.Collections;
using Microsoft.VisualStudio.LanguageServices.Implementation.Interop;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel.InternalElements
{
    [ComVisible(true)]
    [ComDefaultInterface(typeof(EnvDTE.CodeFunction))]
    public sealed partial class CodeAccessorFunction : AbstractCodeElement, EnvDTE.CodeFunction, EnvDTE80.CodeFunction2
    {
        internal static EnvDTE.CodeFunction Create(CodeModelState state, AbstractCodeMember parent, MethodKind kind)
        {
            var newElement = new CodeAccessorFunction(state, parent, kind);
            return (EnvDTE.CodeFunction)ComAggregate.CreateAggregatedObject(newElement);
        }

        private readonly ParentHandle<AbstractCodeMember> _parentHandle;
        private readonly MethodKind _kind;

        private CodeAccessorFunction(CodeModelState state, AbstractCodeMember parent, MethodKind kind)
            : base(state, parent.FileCodeModel)
        {
            Debug.Assert(kind == MethodKind.EventAdd ||
                         kind == MethodKind.EventRaise ||
                         kind == MethodKind.EventRemove ||
                         kind == MethodKind.PropertyGet ||
                         kind == MethodKind.PropertySet);

            _parentHandle = new ParentHandle<AbstractCodeMember>(parent);
            _kind = kind;
        }

        private AbstractCodeMember ParentMember
        {
            get { return _parentHandle.Value; }
        }

        private bool IsPropertyAccessor()
        {
            return _kind == MethodKind.PropertyGet || _kind == MethodKind.PropertySet;
        }

        internal override SyntaxNode LookupNode()
        {
            var parentNode = _parentHandle.Value.LookupNode();
            if (parentNode == null)
            {
                throw Exceptions.ThrowEFail();
            }

            SyntaxNode accessorNode;
            if (!CodeModelService.TryGetAccessorNode(parentNode, _kind, out accessorNode))
            {
                throw Exceptions.ThrowEFail();
            }

            return accessorNode;
        }

        public override EnvDTE.vsCMElement Kind
        {
            get { return EnvDTE.vsCMElement.vsCMElementFunction; }
        }

        public override object Parent
        {
            get { return _parentHandle.Value; }
        }

        public override EnvDTE.CodeElements Children
        {
            get { return EmptyCollection.Create(this.State, this); }
        }

        protected override string GetName()
        {
            return this.ParentMember.Name;
        }

        protected override void SetName(string value)
        {
            this.ParentMember.Name = value;
        }

        protected override string GetFullName()
        {
            return this.ParentMember.FullName;
        }

        public EnvDTE.CodeElements Attributes
        {
            get { return EmptyCollection.Create(this.State, this); }
        }

        public EnvDTE.vsCMAccess Access
        {
            get
            {
                var node = LookupNode();
                return CodeModelService.GetAccess(node);
            }

            set
            {
                UpdateNode(FileCodeModel.UpdateAccess, value);
            }
        }

        public bool CanOverride
        {
            get
            {
                throw new System.NotImplementedException();
            }

            set
            {
                throw new System.NotImplementedException();
            }
        }

        public string Comment
        {
            get
            {
                throw Exceptions.ThrowEFail();
            }

            set
            {
                throw Exceptions.ThrowEFail();
            }
        }

        public string DocComment
        {
            get
            {
                return string.Empty;
            }

            set
            {
                throw Exceptions.ThrowENotImpl();
            }
        }

        public EnvDTE.vsCMFunction FunctionKind
        {
            get
            {
                // TODO: What does VB do here? Handle EventRaise...
                switch (_kind)
                {
                    case MethodKind.PropertyGet:
                    case MethodKind.EventRemove:
                        return EnvDTE.vsCMFunction.vsCMFunctionPropertyGet;

                    case MethodKind.PropertySet:
                    case MethodKind.EventAdd:
                        return EnvDTE.vsCMFunction.vsCMFunctionPropertySet;

                    default:
                        throw Exceptions.ThrowEUnexpected();
                }
            }
        }

        public bool IsGeneric
        {
            get
            {
                if (IsPropertyAccessor())
                {
                    return ((CodeProperty)this.ParentMember).IsGeneric;
                }
                else
                {
                    return ((CodeEvent)this.ParentMember).IsGeneric;
                }
            }
        }

        public EnvDTE80.vsCMOverrideKind OverrideKind
        {
            get
            {
                if (IsPropertyAccessor())
                {
                    return ((CodeProperty)this.ParentMember).OverrideKind;
                }
                else
                {
                    return ((CodeEvent)this.ParentMember).OverrideKind;
                }
            }

            set
            {
                if (IsPropertyAccessor())
                {
                    ((CodeProperty)this.ParentMember).OverrideKind = value;
                }
                else
                {
                    ((CodeEvent)this.ParentMember).OverrideKind = value;
                }
            }
        }

        public bool IsOverloaded
        {
            get { return false; }
        }

        public bool IsShared
        {
            get
            {
                if (IsPropertyAccessor())
                {
                    return ((CodeProperty)this.ParentMember).IsShared;
                }
                else
                {
                    return ((CodeEvent)this.ParentMember).IsShared;
                }
            }

            set
            {
                if (IsPropertyAccessor())
                {
                    ((CodeProperty)this.ParentMember).IsShared = value;
                }
                else
                {
                    ((CodeEvent)this.ParentMember).IsShared = value;
                }
            }
        }

        public bool MustImplement
        {
            get
            {
                if (IsPropertyAccessor())
                {
                    return ((CodeProperty)this.ParentMember).MustImplement;
                }
                else
                {
                    return ((CodeEvent)this.ParentMember).MustImplement;
                }
            }

            set
            {
                if (IsPropertyAccessor())
                {
                    ((CodeProperty)this.ParentMember).MustImplement = value;
                }
                else
                {
                    ((CodeEvent)this.ParentMember).MustImplement = value;
                }
            }
        }

        public EnvDTE.CodeElements Overloads
        {
            get
            {
                throw Exceptions.ThrowEFail();
            }
        }

        public EnvDTE.CodeElements Parameters
        {
            get
            {
                if (IsPropertyAccessor())
                {
                    return ((CodeProperty)this.ParentMember).Parameters;
                }

                throw Exceptions.ThrowEFail();
            }
        }

        public EnvDTE.CodeTypeRef Type
        {
            get
            {
                if (IsPropertyAccessor())
                {
                    return ((CodeProperty)this.ParentMember).Type;
                }

                throw Exceptions.ThrowEFail();
            }

            set
            {
                throw Exceptions.ThrowEFail();
            }
        }

        public EnvDTE.CodeAttribute AddAttribute(string name, string value, object position)
        {
            // TODO(DustinCa): Check VB
            throw Exceptions.ThrowEFail();
        }

        public EnvDTE.CodeParameter AddParameter(string name, object type, object position)
        {
            // TODO(DustinCa): Check VB
            throw Exceptions.ThrowEFail();
        }

        public void RemoveParameter(object element)
        {
            // TODO(DustinCa): Check VB
            throw Exceptions.ThrowEFail();
        }
    }
}
