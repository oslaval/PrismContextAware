using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;

namespace sjb.Framework
{
    /// <summary>
    /// BoundAction<TSource, T0, T1, ..., Tn> provides an easy way to bind a method 
    /// from inputs, where the inputs are bound property paths.
    /// 
    /// <typeparam name="TSource">Type of the object relative to which the propertyPaths are specified.</typeparam>
    /// <typeparam name="Ti">Type of the (i+1)st input to the method.</typeparam>
    /// 
    public abstract class BoundAction
    {
        public static BoundAction<TSource, T0> Create<TSource, T0>(
            TSource source,
            Action<T0> action,
            Expression<Func<TSource, T0>> propertyPath0,
            T0 fallbackValue0 = default(T0)
        )
        {
            return new BoundAction<TSource, T0>(
                source,
                action,
                propertyPath0,
                fallbackValue0
            );
        }


        public static BoundAction<TSource, T0, T1> Create<TSource, T0, T1>(
            TSource source,
            Action<T0, T1> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1)
        )
        {
            return new BoundAction<TSource, T0, T1>(
                source,
                action,
                propertyPath0,
                propertyPath1,
                fallbackValue0,
                fallbackValue1
            );
        }


        public static BoundAction<TSource, T0, T1, T2> Create<TSource, T0, T1, T2>(
            TSource source,
            Action<T0, T1, T2> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2)
        )
        {
            return new BoundAction<TSource, T0, T1, T2>(
                source,
                action,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2
            );
        }


        public static BoundAction<TSource, T0, T1, T2, T3> Create<TSource, T0, T1, T2, T3>(
            TSource source,
            Action<T0, T1, T2, T3> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3)
        )
        {
            return new BoundAction<TSource, T0, T1, T2, T3>(
                source,
                action,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                propertyPath3,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3
            );
        }


        public static BoundAction<TSource, T0, T1, T2, T3, T4> Create<TSource, T0, T1, T2, T3, T4>(
            TSource source,
            Action<T0, T1, T2, T3, T4> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4)
        )
        {
            return new BoundAction<TSource, T0, T1, T2, T3, T4>(
                source,
                action,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                propertyPath3,
                propertyPath4,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4
            );
        }


        public static BoundAction<TSource, T0, T1, T2, T3, T4, T5> Create<TSource, T0, T1, T2, T3, T4, T5>(
            TSource source,
            Action<T0, T1, T2, T3, T4, T5> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            Expression<Func<TSource, T5>> propertyPath5,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4),
            T5 fallbackValue5 = default(T5)
        )
        {
            return new BoundAction<TSource, T0, T1, T2, T3, T4, T5>(
                source,
                action,
                propertyPath0,
                propertyPath1,
                propertyPath2,
                propertyPath3,
                propertyPath4,
                propertyPath5,
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4,
                fallbackValue5
            );
        }

        /*
            }


            public abstract class BoundAction
            {
        */

        private readonly Binding[] Bindings;
        private readonly InputReceptor[] InputReceptors;
        private readonly bool UnderConstruction;

        protected object Input(uint i)
        {
            return InputReceptors[i].Value;
        }

        protected abstract void PerformAction(); // implement this by applying the Action to the InputReceptors. 

        protected BoundAction(object source, string[] paths, object[] fallbackValues)
        {
            UnderConstruction = true;
            Bindings = new Binding[paths.Length];
            InputReceptors = new InputReceptor[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                Bindings[i] = new Binding() { Source = source, Path = new PropertyPath(paths[i]), Mode = BindingMode.OneWay, FallbackValue = fallbackValues[i] };
                InputReceptors[i] = new InputReceptor(this);
                BindingOperations.SetBinding(InputReceptors[i], InputReceptor.ValueProperty, Bindings[i]);
            }
            UnderConstruction = false;
        }

        protected void OnInputChanged()
        {
            if (!UnderConstruction)
            { // SetBinding causes an immediate update. We want to skip it until after the derived class Action is assigned.
                PerformAction();
            }
        }

        private class InputReceptor : DependencyObject
        {
            public InputReceptor(BoundAction boundAction)
            {
                BoundAction = boundAction;
                //this.RegisterPropertyChangedCallback(InputReceptor.ValueProperty, this.ValuePropertyChanged);
            }

            private BoundAction BoundAction { get; set; }

            public object Value
            {
                get { return GetValue(ValueProperty); }
                private set { SetValue(ValueProperty, value); }
            }
            public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(InputReceptor), new PropertyMetadata(null));

            private void ValuePropertyChanged() //DependencyObject sender, DependencyProperty dp)
            {
                BoundAction.OnInputChanged();
            }
        }

    }


    public sealed class BoundAction<TSource, T0> : BoundAction
    {
        private readonly Action<T0> Action;

        public BoundAction(
            TSource source,
            Action<T0> action,
            Expression<Func<TSource, T0>> propertyPath0,
            T0 fallbackValue0 = default(T0)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0)
            },
            new object[] {
                fallbackValue0
            }
        )
        {
            Action = action;
            base.OnInputChanged();
        }

        protected override void PerformAction()
        {
            Action((T0)base.Input(0));
        }
    }


    public sealed class BoundAction<TSource, T0, T1> : BoundAction
    {
        private readonly Action<T0, T1> Action;

        public BoundAction(
            TSource source,
            Action<T0, T1> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1)
            }, new object[] {
                fallbackValue0,
                fallbackValue1
            }
        )
        {
            Action = action;
            base.OnInputChanged();
        }

        protected override void PerformAction()
        {
            Action(
                (T0)base.Input(0),
                (T1)base.Input(1)
            );
        }
    }


    public sealed class BoundAction<TSource, T0, T1, T2> : BoundAction
    {
        private readonly Action<T0, T1, T2> Action;

        public BoundAction(
            TSource source,
            Action<T0, T1, T2> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2
            }
        )
        {
            Action = action;
            base.OnInputChanged();
        }

        protected override void PerformAction()
        {
            Action(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2)
            );
        }
    }


    public sealed class BoundAction<TSource, T0, T1, T2, T3> : BoundAction
    {
        private readonly Action<T0, T1, T2, T3> Action;

        public BoundAction(
            TSource source,
            Action<T0, T1, T2, T3> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2),
                Property.Path(propertyPath3)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3
            }
        )
        {
            Action = action;
            base.OnInputChanged(); // execute this.Action using base.Bindings
        }

        protected override void PerformAction()
        {
            Action(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2),
                (T3)base.Input(3)
            );
        }
    }


    public sealed class BoundAction<TSource, T0, T1, T2, T3, T4> : BoundAction
    {
        private readonly Action<T0, T1, T2, T3, T4> Action;

        public BoundAction(
            TSource source,
            Action<T0, T1, T2, T3, T4> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2),
                Property.Path(propertyPath3),
                Property.Path(propertyPath4)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4
            }
        )
        {
            Action = action;
            base.OnInputChanged();
        }

        protected override void PerformAction()
        {
            Action(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2),
                (T3)base.Input(3),
                (T4)base.Input(4)
            );
        }
    }


    public sealed class BoundAction<TSource, T0, T1, T2, T3, T4, T5> : BoundAction
    {
        private readonly Action<T0, T1, T2, T3, T4, T5> Action;

        public BoundAction(
            TSource source,
            Action<T0, T1, T2, T3, T4, T5> action,
            Expression<Func<TSource, T0>> propertyPath0,
            Expression<Func<TSource, T1>> propertyPath1,
            Expression<Func<TSource, T2>> propertyPath2,
            Expression<Func<TSource, T3>> propertyPath3,
            Expression<Func<TSource, T4>> propertyPath4,
            Expression<Func<TSource, T5>> propertyPath5,
            T0 fallbackValue0 = default(T0),
            T1 fallbackValue1 = default(T1),
            T2 fallbackValue2 = default(T2),
            T3 fallbackValue3 = default(T3),
            T4 fallbackValue4 = default(T4),
            T5 fallbackValue5 = default(T5)
        ) : base(
            source,
            new string[] {
                Property.Path(propertyPath0),
                Property.Path(propertyPath1),
                Property.Path(propertyPath2),
                Property.Path(propertyPath3),
                Property.Path(propertyPath4),
                Property.Path(propertyPath5)
            }, new object[] {
                fallbackValue0,
                fallbackValue1,
                fallbackValue2,
                fallbackValue3,
                fallbackValue4,
                fallbackValue5
            }
        )
        {
            Action = action;
            base.OnInputChanged();
        }

        protected override void PerformAction()
        {
            Action(
                (T0)base.Input(0),
                (T1)base.Input(1),
                (T2)base.Input(2),
                (T3)base.Input(3),
                (T4)base.Input(4),
                (T5)base.Input(5)
            );
        }
    }


}
