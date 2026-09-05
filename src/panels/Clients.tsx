import { FC, useState } from 'react';
import { Button, Div, Group, Header, NavIdProps, Panel, PanelHeader, Placeholder } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';

interface Client {
  id: number;
  name: string;
  phone: string;
}

export interface ClientsProps extends NavIdProps {}

export const Clients: FC<ClientsProps> = ({ id }) => {
  const [clients, setClients] = useState<Client[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);

  const handleAddClient = (values: Record<string, string>) => {
    setClients((currentClients) => [
      ...currentClients,
      {
        id: Date.now(),
        name: values.name.trim(),
        phone: values.phone.trim(),
      },
    ]);
    setIsFormOpen(false);
  };

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Клиенты</div>
      </PanelHeader>
      <Div>
        <Button size="l" mode="primary" onClick={() => setIsFormOpen(true)}>
          Добавить клиента
        </Button>
      </Div>
      <Group header={<Header>Список клиентов</Header>}>
        <Div>
          {clients.length === 0 ? (
            <Placeholder>Пусто — сюда будут добавляться клиенты.</Placeholder>
          ) : (
            clients.map((client) => (
              <Div key={client.id} style={{ padding: '12px 0' }}>
                <strong>{client.name}</strong>
                <div>{client.phone}</div>
              </Div>
            ))
          )}
        </Div>
      </Group>
      <EntityFormModal
        id="add-client"
        title="Новый клиент"
        active={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleAddClient}
        fields={[
          { name: 'name', label: 'Имя', placeholder: 'Имя клиента', required: true },
          { name: 'phone', label: 'Телефон', placeholder: '+7 ...', type: 'tel', required: true },
        ]}
      />
    </Panel>
  );
};

export default Clients;
