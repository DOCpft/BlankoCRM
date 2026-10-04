import { FC, FormEvent, ReactNode, useState } from 'react';
import {
  Button,
  Div,
  FormItem,
  FormLayoutGroup,
  Input,
  ModalPage,
  ModalPageHeader,
  ModalRoot,
  Select,
} from '@vkontakte/vkui';

export interface EntityFormField {
  name: string;
  label: string;
  placeholder?: string;
  type?: string;
  required?: boolean;
  validate?: (value: string) => string | undefined;
  options?: Array<{ value: string; label: string }>;
}

interface EntityFormModalProps {
  id: string;
  title: string;
  fields: EntityFormField[];
  active: boolean;
  onClose: () => void;
  onSubmit: (values: Record<string, string>) => void;
  initialValues?: Record<string, string>;
  onDelete?: () => void;
  children?: ReactNode;
}

export const EntityFormModal: FC<EntityFormModalProps> = ({
  id,
  title,
  fields,
  active,
  onClose,
  onSubmit,
  initialValues = {},
  onDelete,
}) => {
  const [values, setValues] = useState<Record<string, string>>(initialValues);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const nextErrors = fields.reduce<Record<string, string>>((fieldErrors, field) => {
      const value = values[field.name]?.trim() || '';

      if (field.required && !value) {
        fieldErrors[field.name] = 'Заполните поле';
      } else if (field.validate) {
        const error = field.validate(value);
        if (error) fieldErrors[field.name] = error;
      }

      return fieldErrors;
    }, {});

    if (Object.keys(nextErrors).length > 0) {
      setErrors(nextErrors);
      return;
    }

    onSubmit(values);
    setValues({});
  };

  const handleClose = () => {
    setValues({});
    setErrors({});
    onClose();
  };

  return (
    <ModalRoot activeModal={active ? id : null} onClose={handleClose}>
      <ModalPage
        id={id}
        onClose={handleClose}
        header={<ModalPageHeader>{title}</ModalPageHeader>}
      >
        <form onSubmit={handleSubmit}>
          <FormLayoutGroup>
            {fields.map((field) => (
              <FormItem key={field.name} top={field.label} required={field.required}>
                {field.type === 'select' ? (
                  <Select
                    name={field.name}
                    value={values[field.name] || ''}
                    placeholder={field.placeholder}
                    required={field.required}
                    status={errors[field.name] ? 'error' : undefined}
                    options={field.options || []}
                    onChange={(event) =>
                      setValues((currentValues) => ({
                        ...currentValues,
                        [field.name]: event.target.value,
                      }))
                    }
                  />
                ) : (
                  <Input
                    name={field.name}
                    type={field.type}
                    value={values[field.name] || ''}
                    placeholder={field.placeholder}
                    required={field.required}
                    status={errors[field.name] ? 'error' : undefined}
                    onChange={(event) =>
                      setValues((currentValues) => ({
                        ...currentValues,
                        [field.name]: event.target.value,
                      }))
                    }
                  />
                )}
                {errors[field.name] && <div className="crm-form-error">{errors[field.name]}</div>}
              </FormItem>
            ))}
          </FormLayoutGroup>
          <Div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
            <Button size="l" stretched mode="primary" type="submit">
              Сохранить
            </Button>
            <Button size="l" stretched mode="secondary" type="button" onClick={handleClose}>
              Отмена
            </Button>
            {onDelete && (
              <Button size="l" stretched mode="tertiary" appearance="negative" type="button" onClick={onDelete}>
                Удалить
              </Button>
            )}
          </Div>
        </form>
      </ModalPage>
    </ModalRoot>
  );
};

export default EntityFormModal;
