import { Button as VkButton } from '@vkontakte/vkui';
import type { ButtonProps, ButtonSize, ButtonVariant } from './Button.types';

const variantMap: Record<ButtonVariant, 'primary' | 'secondary' | 'tertiary'> = {
  primary: 'primary',
  secondary: 'secondary',
  tertiary: 'tertiary',
  danger: 'tertiary',
};

const sizeMap: Record<ButtonSize, 's' | 'm' | 'l'> = {
  sm: 's',
  md: 'm',
  lg: 'l',
};

export function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  disabled = false,
  fullWidth = false,
  ariaLabel,
  leftIcon,
  rightIcon,
  children,
  ...props
}: ButtonProps) {
  return (
    <VkButton
      mode={variantMap[variant]}
      appearance={variant === 'danger' ? 'negative' : undefined}
      size={sizeMap[size]}
      stretched={fullWidth}
      disabled={disabled || loading}
      aria-label={ariaLabel}
      before={leftIcon}
      after={rightIcon}
      {...props}
    >
      {children}
    </VkButton>
  );
}
