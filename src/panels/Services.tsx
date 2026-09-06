import { FC } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';
import { DataGrid } from '../components/DataGrid';
import { useEntityList } from '../hooks/useEntityList';

interface Service {
  id: string;
  name: string;
  price: number;
  duration: number;
  description?: string;
}

export interface ServicesProps extends NavIdProps {}

const validateNonNegativeNumber = (value: string): string | undefined => {
  const numberValue = Number(value);

  if (!Number.isFinite(numberValue)) return 'Введите число';
  if (numberValue < 0) return 'Значение не может быть отрицательным';

  return undefined;
};

export const Services: FC<ServicesProps> = ({ id }) => {
  const {
    entities: services,
    isFormOpen,
    selectedEntity: selectedService,
    openForm,
    closeForm,
    handleAdd,
    handleEdit,
    handleDelete,
  } = useEntityList<Service>({
    createEntity: (values, entityId) => ({
      id: entityId,
      name: values.name.trim(),
      price: Number(values.price),
      duration: Number(values.duration),
      description: values.description.trim() || undefined,
    }),
    updateEntity: (service, values) => ({
      ...service,
      name: values.name.trim(),
      price: Number(values.price),
      duration: Number(values.duration),
      description: values.description.trim() || undefined,
    }),
  });

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Услуги</div>
      </PanelHeader>
      <Div>
        <Button size="l" mode="primary" onClick={() => openForm()}>
          Добавить услугу
        </Button>
      </Div>
      <Group header={<Header>Список услуг</Header>}>
        <Div>
          {services.length === 0 ? (
            <Placeholder>Пусто — здесь будут добавляться услуги мастера.</Placeholder>
          ) : (
            <DataGrid
              columns={[
                { key: 'name', label: 'Наименование' },
                { key: 'price', label: 'Цена' },
              ]}
              rows={services.map((service) => ({
                id: service.id,
                cells: [service.name, `${service.price} ₽`],
              }))}
              onRowClick={(serviceId) => {
                const service = services.find((item) => item.id === serviceId);
                if (service) openForm(service);
              }}
            />
          )}
        </Div>
      </Group>
      <EntityFormModal
        id="add-service"
        title="Новая услуга"
        active={isFormOpen}
        onClose={closeForm}
        onSubmit={handleAdd}
        fields={[
          { name: 'name', label: 'Название', placeholder: 'Название услуги', required: true, type: 'text' },
          { name: 'price', label: 'Стоимость', placeholder: 'Например, 1500 ₽', required: true, type: 'number', validate: validateNonNegativeNumber },
          { name: 'duration', label: 'Длительность', placeholder: 'Например, 1 час', required: true, type: 'number', validate: validateNonNegativeNumber },
          { name: 'description', label: 'Описание', placeholder: 'Коротко об услуге', required: false, type: 'text' },
        ]}
      />
      {selectedService && (
        <EntityFormModal
          id="edit-service"
          title="Карточка услуги"
          active
          initialValues={{
            name: selectedService.name,
            price: String(selectedService.price),
            duration: String(selectedService.duration),
            description: selectedService.description || '',
          }}
          onClose={closeForm}
          onSubmit={handleEdit}
          onDelete={handleDelete}
          fields={[
            { name: 'name', label: 'Название', placeholder: 'Название услуги', required: true, type: 'text' },
            { name: 'price', label: 'Стоимость', placeholder: 'Например, 1500 ₽', required: true, type: 'number', validate: validateNonNegativeNumber },
            { name: 'duration', label: 'Длительность', placeholder: 'Например, 1 час', required: true, type: 'number', validate: validateNonNegativeNumber },
            { name: 'description', label: 'Описание', placeholder: 'Коротко об услуге', required: false, type: 'text' },
          ]}
        />
      )}
    </Panel>
  );
};

export default Services;
