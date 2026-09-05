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
} from '@vkontakte/vkui';

export interface EntityFormField {
  name: string;
  label: string;
  placeholder?: string;
  type?: string;
  required?: boolean;
}

interface EntityFormModalProps {
  id: string;
  title: string;
  fields: EntityFormField[];
  active: boolean;
  onClose: () => void;
  onSubmit: (values: Record<string, string>) => void;
  children?: ReactNode;
}

export const EntityFormModal: FC<EntityFormModalProps> = ({
  id,
  title,
  fields,
  active,
  onClose,
  onSubmit,
}) => {
  const [values, setValues] = useState<Record<string, string>>({});

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const hasEmptyRequiredField = fields.some(
      (field) => field.required && !values[field.name]?.trim(),
    );

    if (hasEmptyRequiredField) {
      return;
    }

    onSubmit(values);
    setValues({});
  };

  const handleClose = () => {
    setValues({});
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
                <Input
                  name={field.name}
                  type={field.type}
                  value={values[field.name] || ''}
                  placeholder={field.placeholder}
                  required={field.required}
                  onChange={(event) =>
                    setValues((currentValues) => ({
                      ...currentValues,
                      [field.name]: event.target.value,
                    }))
                  }
                />
              </FormItem>
            ))}
          </FormLayoutGroup>
          <Div style={{ display: 'flex', gap: 8 }}>
            <Button size="l" stretched mode="primary" type="submit">
              Сохранить
            </Button>
            <Button size="l" stretched mode="secondary" type="button" onClick={handleClose}>
              Отмена
            </Button>
          </Div>
        </form>
      </ModalPage>
    </ModalRoot>
  );
};

export default EntityFormModal;
