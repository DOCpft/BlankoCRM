import { FC, useState } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';
import { DataGrid } from '../components/DataGrid';

interface Service {
  id: number;
  name: string;
  price: number;
  duration: number;
  description?: string;
}

export interface ServicesProps extends NavIdProps {}

export const Services: FC<ServicesProps> = ({ id }) => {
  const [services, setServices] = useState<Service[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [selectedService, setSelectedService] = useState<Service | null>(null);

  const handleAddService = (values: Record<string, string>) => {
    setServices((currentServices) => [
      ...currentServices,
      {
        id: Date.now(),
        name: values.name.trim(),
        price: Number(values.price),
        duration: Number(values.duration),
        description: values.description ? values.description.trim() : undefined,
      },
    ]);
    setIsFormOpen(false);
  };

  const handleEditService = (values: Record<string, string>) => {
    if (!selectedService) return;

    setServices((currentServices) =>
      currentServices.map((service) =>
        service.id === selectedService.id
          ? {
              ...service,
              name: values.name.trim(),
              price: Number(values.price),
              duration: Number(values.duration),
              description: values.description.trim() || undefined,
            }
          : service,
      ),
    );
    setSelectedService(null);
  };

  const handleDeleteService = () => {
    if (!selectedService) return;
    setServices((currentServices) => currentServices.filter((service) => service.id !== selectedService.id));
    setSelectedService(null);
  };

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Услуги</div>
      </PanelHeader>
      <Div>
        <Button size="l" mode="primary" onClick={() => setIsFormOpen(true)}>
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
                if (service) setSelectedService(service);
              }}
            />
          )}
        </Div>
      </Group>
      <EntityFormModal
        id="add-service"
        title="Новая услуга"
        active={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleAddService}
        fields={[
          { name: 'name', label: 'Название', placeholder: 'Название услуги', required: true, type: 'text' },
          { name: 'price', label: 'Стоимость', placeholder: 'Например, 1500 ₽', required: true, type: 'number' },
          { name: 'duration', label: 'Длительность', placeholder: 'Например, 1 час', required: true, type: 'number' },
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
          onClose={() => setSelectedService(null)}
          onSubmit={handleEditService}
          onDelete={handleDeleteService}
          fields={[
            { name: 'name', label: 'Название', placeholder: 'Название услуги', required: true, type: 'text' },
            { name: 'price', label: 'Стоимость', placeholder: 'Например, 1500 ₽', required: true, type: 'number' },
            { name: 'duration', label: 'Длительность', placeholder: 'Например, 1 час', required: true, type: 'number' },
            { name: 'description', label: 'Описание', placeholder: 'Коротко об услуге', required: false, type: 'text' },
          ]}
        />
      )}
    </Panel>
  );
};

export default Services;
