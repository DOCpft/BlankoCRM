import { FC, useState } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';

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
            services.map((service) => (
              <Div key={service.id} style={{ padding: '12px 0' }}>
                <strong>{service.name}</strong>
                <div>{service.price} ₽</div>
                <div>{service.duration} ч.</div>
              </Div>
            ))
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
    </Panel>
  );
};

export default Services;
