import { FC } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';
import { DataGrid } from '../components/DataGrid';
import { useCrmStore } from '../store/useCrmStore';
import { Service } from '../types';
import { useModal } from '../hooks/useModal';

export interface ServicesProps extends NavIdProps {}

const validateNonNegativeNumber = (value: string): string | undefined => {
  const numberValue = Number(value);

  if (!Number.isFinite(numberValue)) return 'Введите число';
  if (numberValue < 0) return 'Значение не может быть отрицательным';

  return undefined;
};

export const Services: FC<ServicesProps> = ({ id }) => {
  const services = useCrmStore((state) => state.services);
  const addService = useCrmStore((state) => state.addService);
  const updateService = useCrmStore((state) => state.updateService);
  const deleteService = useCrmStore((state) => state.deleteService);
  const serviceModal = useModal<Service>();

  const handleAdd = (values: Record<string, string>) => {
    addService({
      name: values.name.trim(),
      price: Number(values.price),
      duration: Number(values.duration),
      description: values.description.trim() || undefined,
    });
    serviceModal.close();
  };

  const handleEdit = (values: Record<string, string>) => {
    if (!serviceModal.selected) return;
    updateService(serviceModal.selected.id, {
      name: values.name.trim(),
      price: Number(values.price),
      duration: Number(values.duration),
      description: values.description.trim() || undefined,
    });
    serviceModal.close();
  };

  const handleDelete = () => {
    if (!serviceModal.selected) return;
    deleteService(serviceModal.selected.id);
    serviceModal.close();
  };

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Услуги</div>
      </PanelHeader>
      <Div>
        <Button size="l" mode="primary" onClick={() => serviceModal.open()}>
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
                if (service) serviceModal.open(service);
              }}
            />
          )}
        </Div>
      </Group>
      <EntityFormModal
        id="add-service"
        title="Новая услуга"
        active={serviceModal.isOpen && !serviceModal.selected}
        onClose={serviceModal.close}
        onSubmit={handleAdd}
        fields={[
          { name: 'name', label: 'Название', placeholder: 'Название услуги', required: true, type: 'text' },
          { name: 'price', label: 'Стоимость', placeholder: 'Например, 1500 ₽', required: true, type: 'number', validate: validateNonNegativeNumber },
          { name: 'duration', label: 'Длительность', placeholder: 'Например, 1 час', required: true, type: 'number', validate: validateNonNegativeNumber },
          { name: 'description', label: 'Описание', placeholder: 'Коротко об услуге', required: false, type: 'text' },
        ]}
      />
      {serviceModal.selected && (
        <EntityFormModal
          id="edit-service"
          title="Карточка услуги"
          active={serviceModal.isOpen}
          initialValues={{
            name: serviceModal.selected.name,
            price: String(serviceModal.selected.price),
            duration: String(serviceModal.selected.duration),
            description: serviceModal.selected.description || '',
          }}
          onClose={serviceModal.close}
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
