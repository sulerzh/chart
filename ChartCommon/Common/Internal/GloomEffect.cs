﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class GloomEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GloomEffect), 0);
        public static readonly DependencyProperty GloomIntensityProperty = DependencyProperty.Register("GloomIntensity", typeof(double), typeof(GloomEffect), new PropertyMetadata((object)1.0, ShaderEffect.PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty BaseIntensityProperty = DependencyProperty.Register("BaseIntensity", typeof(double), typeof(GloomEffect), new PropertyMetadata((object)0.5, ShaderEffect.PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty GloomSaturationProperty = DependencyProperty.Register("GloomSaturation", typeof(double), typeof(GloomEffect), new PropertyMetadata((object)0.2, ShaderEffect.PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty BaseSaturationProperty = DependencyProperty.Register("BaseSaturation", typeof(double), typeof(GloomEffect), new PropertyMetadata((object)1.0, ShaderEffect.PixelShaderConstantCallback(3)));

        public Brush Input
        {
            get
            {
                return (Brush)this.GetValue(GloomEffect.InputProperty);
            }
            set
            {
                this.SetValue(GloomEffect.InputProperty, (object)value);
            }
        }

        public double GloomIntensity
        {
            get
            {
                return (double)this.GetValue(GloomEffect.GloomIntensityProperty);
            }
            set
            {
                this.SetValue(GloomEffect.GloomIntensityProperty, (object)value);
            }
        }

        public double BaseIntensity
        {
            get
            {
                return (double)this.GetValue(GloomEffect.BaseIntensityProperty);
            }
            set
            {
                this.SetValue(GloomEffect.BaseIntensityProperty, (object)value);
            }
        }

        public double GloomSaturation
        {
            get
            {
                return (double)this.GetValue(GloomEffect.GloomSaturationProperty);
            }
            set
            {
                this.SetValue(GloomEffect.GloomSaturationProperty, (object)value);
            }
        }

        public double BaseSaturation
        {
            get
            {
                return (double)this.GetValue(GloomEffect.BaseSaturationProperty);
            }
            set
            {
                this.SetValue(GloomEffect.BaseSaturationProperty, (object)value);
            }
        }

        public GloomEffect()
        {
            this.PixelShader = new PixelShader()
            {
                UriSource = new Uri("/VisualizationChartCommon;component/PixelShaderEffects/Gloom.ps", UriKind.Relative)
            };
            this.UpdateShaderValue(GloomEffect.InputProperty);
            this.UpdateShaderValue(GloomEffect.GloomIntensityProperty);
            this.UpdateShaderValue(GloomEffect.BaseIntensityProperty);
            this.UpdateShaderValue(GloomEffect.GloomSaturationProperty);
            this.UpdateShaderValue(GloomEffect.BaseSaturationProperty);
        }
    }
}
